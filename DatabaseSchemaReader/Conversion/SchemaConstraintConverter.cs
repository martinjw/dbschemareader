using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    static class SchemaConstraintConverter
    {
        /// <summary>
        /// Converts the "PRIMARY_KEYS", "FOREIGN_KEYS" and "UNIQUE_KEYS" DataTables into <see cref="DatabaseConstraint"/> objects
        /// </summary>
        public static List<DatabaseConstraint> Constraints(DataTable dt, ConstraintType constraintType)
        {
            return Constraints(dt, constraintType, null);
        }
        /// <summary>
        /// Converts the "PRIMARY_KEYS", "FOREIGN_KEYS" and "UNIQUE_KEYS" DataTables into <see cref="DatabaseConstraint"/> objects
        /// </summary>
        public static List<DatabaseConstraint> Constraints(DataTable dt, ConstraintType constraintType, string tableName)
        {
            List<DatabaseConstraint> list = new List<DatabaseConstraint>();
            if (dt.Rows.Count == 0) return list; //nothing to do
            //all same, my custom sql
            string key = "CONSTRAINT_NAME";
            string tableKey = "TABLE_NAME";
            string columnKey = "COLUMN_NAME";
            string ordinalKey = "ORDINAL_POSITION";
            string refersToKey = "UNIQUE_CONSTRAINT_NAME";
            string refersToTableKey = "FK_TABLE";
            const string expression = "EXPRESSION";
            string deleteRuleKey = "DELETE_RULE";
            string updateRuleKey = "UPDATE_RULE";
            //oracle
            if (!dt.Columns.Contains(key)) key = "FOREIGN_KEY_CONSTRAINT_NAME";
            if (!dt.Columns.Contains(tableKey)) tableKey = "FOREIGN_KEY_TABLE_NAME";
            if (!dt.Columns.Contains(refersToTableKey)) refersToTableKey = "PRIMARY_KEY_TABLE_NAME";
            if (!dt.Columns.Contains(refersToKey)) refersToKey = "PRIMARY_KEY_CONSTRAINT_NAME";
            //devart.data.postgresql
            if (!dt.Columns.Contains(key)) key = "NAME";
            if (!dt.Columns.Contains(tableKey)) tableKey = "TABLE";

            //firebird
            if (!dt.Columns.Contains(key)) key = "PK_NAME";
            if (!dt.Columns.Contains(refersToTableKey)) refersToTableKey = "REFERENCED_TABLE_NAME";
            //sqlite
            if (!dt.Columns.Contains(columnKey)) columnKey = "FKEY_FROM_COLUMN";
            if (!dt.Columns.Contains(ordinalKey)) ordinalKey = "FKEY_FROM_ORDINAL_POSITION";
            if (!dt.Columns.Contains(refersToTableKey)) refersToTableKey = "FKEY_TO_TABLE";
            if (!dt.Columns.Contains(refersToKey)) refersToKey = null;
            if (!dt.Columns.Contains(refersToTableKey)) refersToTableKey = null;
            if (!dt.Columns.Contains(deleteRuleKey)) deleteRuleKey = null;
            if (!dt.Columns.Contains(updateRuleKey)) updateRuleKey = null;
            //not present if separate foreign key columns
            if (!dt.Columns.Contains(columnKey)) columnKey = null;
            if (!dt.Columns.Contains(ordinalKey)) ordinalKey = null;


            //sort it (unless it's a check constraint)
            CreateDefaultView(dt, tableKey, ordinalKey, constraintType, tableName);

            foreach (DataRowView row in dt.DefaultView)
            {
                string name = row[key].ToString();
                //constraints may be on multiple columns, each as sep row.
                DatabaseConstraint c = FindConstraint(list, name);
                if (c == null)
                {
                    c = new DatabaseConstraint(); //it's a new constraint
                    c.Name = name;
                    c.TableName = row[tableKey].ToString();
                    c.ConstraintType = constraintType;
                    list.Add(c);
                    if (constraintType == ConstraintType.Check)
                    {
                        c.Expression = row[expression].ToString();
                    }
                    else
                    {
                        c.RefersToConstraint = AddRefersToConstraint(row, refersToKey);
                        if (!string.IsNullOrEmpty(refersToTableKey))
                            c.RefersToTable = row[refersToTableKey].ToString();
                        c.DeleteRule = AddDeleteUpdateRule(row, deleteRuleKey);
                        c.UpdateRule = AddDeleteUpdateRule(row, updateRuleKey);
                    }
                }
                AddConstraintColumns(row, columnKey, constraintType, c);
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
            if (!string.IsNullOrEmpty(rule) && !rule.Equals("NO ACTION", StringComparison.OrdinalIgnoreCase))
                return rule;
            return null;
        }

        /// <summary>
        /// Adds the foreign key columns. Add the foreign keys first.
        /// </summary>
        public static void AddForeignKeyColumns(DataTable dt, DatabaseTable table)
        {
            if (dt.Rows.Count == 0) return; //no rows to add
            if (table.ForeignKeys.Count == 0) return; //no fks to match
            string key = "CONSTRAINT_NAME";
            string tableKey = "TABLE_NAME";
            string columnKey = "COLUMN_NAME";
            if (!dt.Columns.Contains(key)) key = "foreignkey";
            if (!dt.Columns.Contains(tableKey)) tableKey = "table";
            if (!dt.Columns.Contains(columnKey)) columnKey = "name";

            //this could be more than one table, so filter the view
            dt.DefaultView.RowFilter = "[" + tableKey + "] = '" + table.Name + "'";
            foreach (DataRowView row in dt.DefaultView)
            {
                string name = row[key].ToString();
                DatabaseConstraint c = table.ForeignKeys.Find(delegate(DatabaseConstraint f) { return f.Name == name; });
                if (c == null)
                {
                    continue; //throw here?
                }
                string col = row[columnKey].ToString();
                //only add it once
                if (!c.Columns.Contains(col))
                    c.Columns.Add(col);
            }
        }

        /// <summary>
        /// Converts the "IndexColumns" DataTable into <see cref="DatabaseIndex"/> objects
        /// </summary>
        public static List<DatabaseIndex> Indexes(DataTable dt)
        {
            return Indexes(dt, null);
        }

        /// <summary>
        /// Converts the "IndexColumns" DataTable into <see cref="DatabaseIndex"/> objects
        /// </summary>
        public static List<DatabaseIndex> Indexes(DataTable dt, string tableName)
        {
            List<DatabaseIndex> list = new List<DatabaseIndex>();
            Indexes(dt, tableName, list);
            return list;
        }

        /// <summary>
        /// Converts the "IndexColumns" DataTable into <see cref="DatabaseIndex"/> objects
        /// </summary>
        public static void Indexes(DataTable dt, string tableName, List<DatabaseIndex> list)
        {
            if (list == null) list = new List<DatabaseIndex>();

            //Npgsql
            if (dt.Columns.Count == 0) return;

            string uniqueKey = "UNIQUE";
            string primaryKey = "PRIMARY";

            //sql server
            string key = "CONSTRAINT_NAME";
            string tableKey = "TABLE_NAME";
            string schemaKey = "TABLE_SCHEMA";
            string columnKey = "COLUMN_NAME";
            string ordinalKey = "ORDINAL_POSITION";
            //oracle
            string typekey = "INDEX_TYPE";
            if (!dt.Columns.Contains(schemaKey)) schemaKey = "INDEX_SCHEMA";
            if (!dt.Columns.Contains(schemaKey)) schemaKey = "OWNER";
            if (!dt.Columns.Contains(key)) key = "INDEX_NAME";
            if (!dt.Columns.Contains(schemaKey)) schemaKey = "INDEX_OWNER";
            if (!dt.Columns.Contains(ordinalKey)) ordinalKey = "COLUMN_POSITION";
            if (!dt.Columns.Contains(uniqueKey)) uniqueKey = "UNIQUENESS";
            //mysql
            if (!dt.Columns.Contains(schemaKey)) schemaKey = "INDEX_SCHEMA";
            //Devart.Data.Oracle
            if (!dt.Columns.Contains(key)) key = "INDEX"; //IndexColumns
            if (!dt.Columns.Contains(key)) key = "NAME"; //Indexes
            if (!dt.Columns.Contains(uniqueKey)) uniqueKey = "ISUNIQUE";
            if (!dt.Columns.Contains(schemaKey)) schemaKey = "SCHEMA";
            if (!dt.Columns.Contains(tableKey)) tableKey = "TABLE";
            if (!dt.Columns.Contains(ordinalKey)) ordinalKey = "POSITION";
            if (!dt.Columns.Contains(columnKey)) columnKey = "NAME";
            //devart.data.postgresql
            if (!dt.Columns.Contains(key)) key = "indexname";
            //sqlite
            if (!dt.Columns.Contains(primaryKey)) primaryKey = "PRIMARY_KEY";
            //postgresql
            if (!dt.Columns.Contains(ordinalKey)) ordinalKey = null;
            //sqlserver 2008 - HEAP CLUSTERED NONCLUSTERED XML SPATIAL
            //sys_indexes has is_unique but it's not exposed. 
            if (!dt.Columns.Contains(typekey)) typekey = "type_desc";

            //pre 2008 sql server
            if (!dt.Columns.Contains(typekey)) typekey = null;

            //indexes and not indexcolumns
            if (!dt.Columns.Contains(columnKey)) columnKey = null;
            if (!dt.Columns.Contains(uniqueKey)) uniqueKey = null;
            if (!dt.Columns.Contains(primaryKey)) primaryKey = null;

            if (!string.IsNullOrEmpty(ordinalKey))
                dt.DefaultView.Sort = ordinalKey;
            //this could be more than one table, so filter the view
            if (!string.IsNullOrEmpty(tableName))
                dt.DefaultView.RowFilter = "[" + tableKey + "] = '" + tableName + "'";

            foreach (DataRowView row in dt.DefaultView)
            {
                string name = row[key].ToString();
                string schema = row[schemaKey].ToString();
                DatabaseIndex c = list.Find(delegate(DatabaseIndex f) { return f.Name == name && f.SchemaOwner == schema; });
                if (c == null)
                {
                    c = new DatabaseIndex();
                    c.Name = name;
                    c.SchemaOwner = schema;
                    c.TableName = row[tableKey].ToString();
                    if (typekey != null)
                        c.IndexType = row[typekey].ToString();
                    if (FindBoolean(row, uniqueKey, "UNIQUE"))
                    {
                        c.IsUnique = true;
                        c.IndexType = "UNIQUE";
                    }
                    if (FindBoolean(row, primaryKey, string.Empty))
                        c.IndexType = "PRIMARY"; //primary keys should be unique too
                    list.Add(c);
                }
                if (string.IsNullOrEmpty(columnKey)) continue;

                string colName = row[columnKey].ToString();
                if (string.IsNullOrEmpty(colName)) continue;
                DatabaseColumn column = new DatabaseColumn();
                column.Name = colName;
                if (!string.IsNullOrEmpty(ordinalKey))
                {
                    int ordinal = Convert.ToInt32(row[ordinalKey], CultureInfo.CurrentCulture);
                    column.Ordinal = ordinal;
                }
                c.Columns.Add(column);
            }
        }

        private static bool FindBoolean(DataRowView row, string key, string trueText)
        {
            if (key == null) return false;
            var o = row[key];
            if (o == DBNull.Value) return false;
            if (o.GetType() == typeof(string))
            {
                return (o.Equals(trueText));
            }
            return (bool)o;
        }

        /// <summary>
        /// Converts the "Triggers" DataTable into <see cref="DatabaseTrigger"/> objects
        /// </summary>
        public static List<DatabaseTrigger> Triggers(DataTable dt)
        {
            return Triggers(dt, null);
        }

        /// <summary>
        /// Converts the "Triggers" DataTable into <see cref="DatabaseTrigger"/> objects
        /// </summary>
        public static List<DatabaseTrigger> Triggers(DataTable dt, string tableName)
        {
            List<DatabaseTrigger> list = new List<DatabaseTrigger>();
            if (dt.Columns.Count == 0) return list;
            //sql server
            string key = "TRIGGER_NAME";
            string tableKey = "TABLE_NAME";
            string bodyKey = "TRIGGER_BODY";
            string eventKey = "TRIGGERING_EVENT";
            string triggerTypeKey = "TRIGGER_TYPE";
            string ownerKey = "OWNER";
            //firebird
            if (!dt.Columns.Contains(ownerKey)) ownerKey = null;
            if (!dt.Columns.Contains(bodyKey)) bodyKey = "SOURCE";
            if (!dt.Columns.Contains(eventKey)) eventKey = "TRIGGER_TYPE";
            if (!dt.Columns.Contains(bodyKey)) bodyKey = "BODY";

            if (!dt.Columns.Contains(tableKey)) tableKey = null;
            if (!dt.Columns.Contains(bodyKey)) bodyKey = null;
            if (!dt.Columns.Contains(eventKey)) eventKey = null;
            if (!dt.Columns.Contains(triggerTypeKey)) triggerTypeKey = null;

            //this could be more than one table, so filter the view
            if (!string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(tableKey))
                dt.DefaultView.RowFilter = "[" + tableKey + "] = '" + tableName + "'";

            foreach (DataRowView row in dt.DefaultView)
            {
                string name = row[key].ToString();
                DatabaseTrigger c = list.Find(delegate(DatabaseTrigger f) { return f.Name == name; });
                if (c == null)
                {
                    c = new DatabaseTrigger();
                    c.Name = name;
                    if (ownerKey != null)
                        c.SchemaOwner = row[ownerKey].ToString();
                    list.Add(c);
                }
                if (!string.IsNullOrEmpty(tableKey))
                    c.TableName = row[tableKey].ToString();
                if (!string.IsNullOrEmpty(bodyKey))
                    c.TriggerBody = row[bodyKey].ToString();
                if (!string.IsNullOrEmpty(eventKey))
                    c.TriggerEvent = row[eventKey].ToString();
                if (triggerTypeKey != null)
                {
                    c.TriggerType = row[triggerTypeKey].ToString();
                }
            }
            return list;
        }

        /// <summary>
        /// Converts the "IdentityColumns" DataTable by updating the Identity column in a table
        /// </summary>
        public static void AddIdentity(DataTable dt, DatabaseTable table)
        {
            foreach (DataRow row in dt.Rows)
            {
                string tableName = row["TableName"].ToString();
                string colName = row["ColumnName"].ToString();
                if (!tableName.Equals(table.Name, StringComparison.OrdinalIgnoreCase))
                    continue;
                foreach (DatabaseColumn col in table.Columns)
                {
                    if (col.Name.Equals(colName, StringComparison.OrdinalIgnoreCase))
                    {
                        col.IsIdentity = true;
                        //col.IsPrimaryKey = true;
                        break;
                    }
                }
            }
        }
    }
}
