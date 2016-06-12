using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DatabaseSchemaReader.Conversion.KeyMaps;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    class SchemaConstraintConverter
    {
        private readonly List<DatabaseConstraint> _constraints;

        public SchemaConstraintConverter(DataTable dt, ConstraintType constraintType)
        {
            _constraints = Constraints(dt, constraintType, null);
        }

        public List<DatabaseConstraint> Constraints()
        {
            return _constraints;
        }
        public List<DatabaseConstraint> Constraints(string tableName, string schemaName)
        {
            //match by table name and (nullable) schema
            return _constraints.Where(x =>
                string.Equals(x.TableName, tableName) &&
                string.Equals(x.SchemaOwner, schemaName)).ToList();
        }

        /// <summary>
        /// Converts the "PRIMARY_KEYS", "FOREIGN_KEYS" and "UNIQUE_KEYS" DataTables into <see cref="DatabaseConstraint"/> objects
        /// </summary>
        private static List<DatabaseConstraint> Constraints(DataTable dt, ConstraintType constraintType, string tableName)
        {
            var list = new List<DatabaseConstraint>();
            if (dt.Rows.Count == 0) return list; //nothing to do

            var constraintKeyMap = new ConstraintKeyMap(dt, constraintType);

            //sort it (unless it's a check constraint)
            CreateDefaultView(dt, constraintKeyMap.TableKey, constraintKeyMap.OrdinalKey, constraintType, tableName);

            foreach (DataRowView row in dt.DefaultView)
            {
                string name = null;
                DatabaseConstraint constraint = null;
                var nameKey = constraintKeyMap.Key;
                var constraintTableName = row[constraintKeyMap.TableKey].ToString();
                string schemaName = null;
                if (!string.IsNullOrEmpty(constraintKeyMap.SchemaKey))
                {
                    schemaName = row[constraintKeyMap.SchemaKey].ToString();
                }
                if (!string.IsNullOrEmpty(nameKey))
                {
                    name = row[nameKey].ToString();
                    constraint = FindConstraint(list, name, constraintTableName, schemaName);
                }
                //constraints may be on multiple columns, each as sep row.
                if (constraint == null)
                {
                    constraint = new DatabaseConstraint(); //it's a new constraint
                    constraint.Name = name;
                    constraint.TableName = constraintTableName;
                    constraint.SchemaOwner = schemaName;
                    constraint.ConstraintType = constraintType;
                    list.Add(constraint);
                    if ((constraintType == ConstraintType.Check || constraintType == ConstraintType.Default)
                        && constraintKeyMap.ExpressionKey != null)
                    {
                        constraint.Expression = row[constraintKeyMap.ExpressionKey].ToString();
                        if (constraintType == ConstraintType.Check)
                        {
                            //check constraint doesn't explicitly reference columns
                            continue;
                        }
                    }
                    constraint.RefersToConstraint = AddRefersToConstraint(row, constraintKeyMap.RefersToKey);
                    if (!string.IsNullOrEmpty(constraintKeyMap.RefersToTableKey))
                    {
                        //foreign key to table/schema
                        constraint.RefersToTable = row[constraintKeyMap.RefersToTableKey].ToString();
                        if (!string.IsNullOrEmpty(constraintKeyMap.RefersToSchemaKey))
                            constraint.RefersToSchema = row[constraintKeyMap.RefersToSchemaKey].ToString();
                    }
                    constraint.DeleteRule = AddDeleteUpdateRule(row, constraintKeyMap.DeleteRuleKey);
                    constraint.UpdateRule = AddDeleteUpdateRule(row, constraintKeyMap.UpdateRuleKey);
                }
                AddConstraintColumns(row, constraintKeyMap.ColumnKey, constraintType, constraint);
            }

            return list;
        }

        private static void CreateDefaultView(DataTable dt, string tableKey, string ordinalKey, ConstraintType constraintType, string tableName)
        {
            if (constraintType != ConstraintType.Check && !string.IsNullOrEmpty(ordinalKey))
                dt.DefaultView.Sort = ordinalKey;
            //this could be more than one table, so filter the view
            if (!string.IsNullOrEmpty(tableName))
                dt.DefaultView.RowFilter = "[" + tableKey + "] = '" + tableName + "'";
        }

        private static DatabaseConstraint FindConstraint(List<DatabaseConstraint> list, string name, string constraintTableName, string schemaName)
        {
            return list.Find(delegate(DatabaseConstraint f) { return f.Name == name && f.TableName == constraintTableName && f.SchemaOwner == schemaName; });
        }

        private static string AddRefersToConstraint(DataRowView row, string refersToKey)
        {
            if (!string.IsNullOrEmpty(refersToKey) && row[refersToKey] != DBNull.Value)
                return row[refersToKey].ToString();
            return null;
        }

        private static void AddConstraintColumns(DataRowView row, string columnKey, ConstraintType constraintType, DatabaseConstraint constraint)
        {
            if (constraintType == ConstraintType.Check || string.IsNullOrEmpty(columnKey)) return;
            string col = row[columnKey].ToString();
            constraint.Columns.Add(col); //assume they are in the right order
        }

        private static string AddDeleteUpdateRule(DataRowView row, string deleteUpdateRuleKey)
        {
            if (string.IsNullOrEmpty(deleteUpdateRuleKey)) return null;

            string rule = row[deleteUpdateRuleKey].ToString();
            //translate DB2 numbers
            if (rule == "0") rule = "CASCADE";
            else if (rule == "1") rule = "RESTRICT";
            else if (rule == "2") rule = "SET NULL";
            else if (rule == "3") rule = "NO ACTION";
            if (!string.IsNullOrEmpty(rule) && !rule.Equals("NO ACTION", StringComparison.OrdinalIgnoreCase))
                return rule;
            return null;
        }


        public static IList<DatabaseColumn> ConvertIdentity(DataTable dt)
        {
            var result = new List<DatabaseColumn>();
            bool hasSeedInfo = dt.Columns.Contains("IdentitySeed");
            bool hasIncrementInfo = dt.Columns.Contains("IdentityIncrement");
            bool hasIdentityOptions = dt.Columns.Contains("IDENTITY_OPTIONS");
            bool hasGeneratedType = dt.Columns.Contains("GENERATION_TYPE");
            foreach (DataRow row in dt.Rows)
            {
                string schemaOwner = row["SchemaOwner"].ToString();
                string tableName = row["TableName"].ToString();
                string colName = row["ColumnName"].ToString();
                var col = new DatabaseColumn
                          {
                              SchemaOwner = schemaOwner,
                              TableName = tableName,
                              Name = colName,
                          };
                result.Add(col);
                AddIdentity(col, row, hasSeedInfo, hasIncrementInfo, hasIdentityOptions, hasGeneratedType);
            }
            return result;
        }


        /// <summary>
        /// Converts the "IdentityColumns" DataTable by updating the Identity column in a table
        /// </summary>
        public static void AddIdentity(DataTable dt, DatabaseTable table)
        {
            bool hasSeedInfo = dt.Columns.Contains("IdentitySeed");
            bool hasIncrementInfo = dt.Columns.Contains("IdentityIncrement");
            bool hasIdentityOptions = dt.Columns.Contains("IDENTITY_OPTIONS");
            bool hasGeneratedType = dt.Columns.Contains("GENERATION_TYPE");
            foreach (DataRow row in dt.Rows)
            {
                string tableName = row["TableName"].ToString();
                if (!tableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase))
                    continue;
                string colName = row["ColumnName"].ToString();
                var col = table.FindColumn(colName);
                if (col != null)
                {
                    AddIdentity(col, row, hasSeedInfo, hasIncrementInfo, hasIdentityOptions, hasGeneratedType);
                    //col.IsPrimaryKey = true;
                }
            }
        }

        private static void AddIdentity(DatabaseColumn col, DataRow row, bool hasSeedInfo, bool hasIncrementInfo,
            bool hasIdentityOptions, bool hasGeneratedType)
        {
            col.IsAutoNumber = true;
            col.IdentityDefinition = new DatabaseColumnIdentity();
            if (hasSeedInfo)
                col.IdentityDefinition.IdentitySeed = long.Parse(row["IdentitySeed"].ToString());
            if (hasIncrementInfo)
                col.IdentityDefinition.IdentityIncrement = long.Parse(row["IdentityIncrement"].ToString());
            if (hasIdentityOptions)
            {
                var options = row["IDENTITY_OPTIONS"].ToString();
                ParseIdentityOptions(col.IdentityDefinition, options);
            }
            if (hasGeneratedType)
            {
                if (string.Equals(row["GENERATION_TYPE"].ToString(), "BY DEFAULT",
                    StringComparison.OrdinalIgnoreCase))
                {
                    col.IdentityDefinition.IdentityByDefault = true;
                }
            }
        }

        private static void ParseIdentityOptions(DatabaseColumnIdentity identityDefinition, string options)
        {
            //START WITH: 1, INCREMENT BY: 1, MAX_VALUE: 9999999999999999999999999999, MIN_VALUE: 1, CYCLE_FLAG: N, CACHE_SIZE: 20, ORDER_FLAG: N
            //defensive in case format changes
            if (string.IsNullOrEmpty(options)) return;

            var number = ExtractBetween(options, "START WITH: ", ',');
            if (string.IsNullOrEmpty(number)) return;
            long seed;
            if (long.TryParse(number, out seed))
            {
                identityDefinition.IdentitySeed = seed;
            }

            number = ExtractBetween(options, "INCREMENT BY: ", ',');
            if (string.IsNullOrEmpty(number)) return;
            if (long.TryParse(number, out seed))
            {
                identityDefinition.IdentityIncrement = seed;
            }

        }

        private static string ExtractBetween(string haystack, string prefix, char suffix)
        {
            var start = haystack.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (start == -1) return null;
            start = start + prefix.Length;
            var end = haystack.IndexOf(suffix, start);
            return haystack.Substring(start, end - start);
        }

        public static IList<DatabaseColumn> ConvertComputed(DataTable dt)
        {
            var result = new List<DatabaseColumn>();
            foreach (DataRow row in dt.Rows)
            {
                string schemaOwner = row["SchemaOwner"].ToString();
                var tableName = row["TABLENAME"].ToString();
                var colName = row["COLUMNNAME"].ToString();
                var col = new DatabaseColumn
                {
                    SchemaOwner = schemaOwner,
                    TableName = tableName,
                    Name = colName,
                };
                result.Add(col);
                col.ComputedDefinition = row["COMPUTEDDEFINITION"].ToString();
                //remove the default value - it's readonly!
                col.DefaultValue = null;
            }
            return result;
        }

        public static void AddComputed(DataTable dt, DatabaseTable table)
        {
            foreach (DataRow row in dt.Rows)
            {
                var tableName = row["TABLENAME"].ToString();
                if (!tableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase))
                    continue;
                var colName = row["COLUMNNAME"].ToString();
                var col = table.FindColumn(colName);
                if (col != null)
                {
                    col.ComputedDefinition = row["COMPUTEDDEFINITION"].ToString();
                    //remove the default value - it's readonly!
                    col.DefaultValue = null;
                }
            }
        }
    }
}
