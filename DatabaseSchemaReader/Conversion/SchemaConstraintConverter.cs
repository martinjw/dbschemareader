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
        public List<DatabaseConstraint> Constraints(string tableName)
        {
            return _constraints.Where(x => x.TableName.Equals(tableName)).ToList();
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
                if (!string.IsNullOrEmpty(nameKey))
                {
                    name = row[nameKey].ToString();
                    constraint = FindConstraint(list, name);
                }
                //constraints may be on multiple columns, each as sep row.
                if (constraint == null)
                {
                    constraint = new DatabaseConstraint(); //it's a new constraint
                    constraint.Name = name;
                    constraint.TableName = row[constraintKeyMap.TableKey].ToString();
                    constraint.ConstraintType = constraintType;
                    list.Add(constraint);
                    if (constraintType == ConstraintType.Check && constraintKeyMap.ExpressionKey != null)
                    {
                        constraint.Expression = row[constraintKeyMap.ExpressionKey].ToString();
                        continue;
                    }
                    constraint.RefersToConstraint = AddRefersToConstraint(row, constraintKeyMap.RefersToKey);
                    if (!string.IsNullOrEmpty(constraintKeyMap.RefersToTableKey))
                        constraint.RefersToTable = row[constraintKeyMap.RefersToTableKey].ToString();
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

        private static DatabaseConstraint FindConstraint(List<DatabaseConstraint> list, string name)
        {
            return list.Find(delegate(DatabaseConstraint f) { return f.Name == name; });
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



        /// <summary>
        /// Converts the "IdentityColumns" DataTable by updating the Identity column in a table
        /// </summary>
        public static void AddIdentity(DataTable dt, DatabaseTable table)
        {
            bool hasSeedInfo = dt.Columns.Contains("IdentitySeed");
            bool hasIncrementInfo = dt.Columns.Contains("IdentityIncrement");
            foreach (DataRow row in dt.Rows)
            {
                string tableName = row["TableName"].ToString();
                if (!tableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase))
                    continue;
                string colName = row["ColumnName"].ToString();
                var col = table.FindColumn(colName);
                if (col != null)
                {
                    col.IsIdentity = true;
                    if (hasSeedInfo)
                        col.IdentitySeed = long.Parse(row["IdentitySeed"].ToString());
                    if (hasIncrementInfo)
                        col.IdentityIncrement = long.Parse(row["IdentityIncrement"].ToString());
                    //col.IsPrimaryKey = true;
                }
            }
        }

        public static void AddComputed(DataTable dt, DatabaseTable table)
        {
            foreach (DataRow row in dt.Rows)
            {
                var tableName = row["TableName"].ToString();
                if (!tableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase))
                    continue;
                var colName = row["ColumnName"].ToString();
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
