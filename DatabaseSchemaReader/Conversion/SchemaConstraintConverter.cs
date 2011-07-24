using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using DatabaseSchemaReader.Conversion.KeyMaps;
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

            ConstraintKeyMap constraintKeyMap = new ConstraintKeyMap(dt, constraintType);

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

            var indexKeyMap = new IndexKeyMap(dt);

            if (!string.IsNullOrEmpty(indexKeyMap.OrdinalKey))
                dt.DefaultView.Sort = indexKeyMap.OrdinalKey;
            //this could be more than one table, so filter the view
            if (!string.IsNullOrEmpty(tableName))
                dt.DefaultView.RowFilter = string.Format(CultureInfo.InvariantCulture, "[{0}] = '{1}'", indexKeyMap.TableKey, tableName);

            foreach (DataRowView row in dt.DefaultView)
            {
                string name = row[indexKeyMap.Key].ToString();
                if (string.IsNullOrEmpty(name)) continue; //all indexes should have a name
                string schema = !string.IsNullOrEmpty(indexKeyMap.SchemaKey) ? row[indexKeyMap.SchemaKey].ToString() : string.Empty;
                DatabaseIndex c = list.Find(delegate(DatabaseIndex f) { return f.Name == name && f.SchemaOwner == schema; });
                if (c == null)
                {
                    c = new DatabaseIndex();
                    c.Name = name;
                    c.SchemaOwner = schema;
                    c.TableName = row[indexKeyMap.TableKey].ToString();
                    if (indexKeyMap.Typekey != null)
                        c.IndexType = row[indexKeyMap.Typekey].ToString();
                    if (FindBoolean(row, indexKeyMap.UniqueKey, "UNIQUE"))
                    {
                        c.IsUnique = true;
                        c.IndexType = "UNIQUE";
                    }
                    if (FindBoolean(row, indexKeyMap.PrimaryKey, string.Empty))
                        c.IndexType = "PRIMARY"; //primary keys should be unique too
                    list.Add(c);
                }
                if (string.IsNullOrEmpty(indexKeyMap.ColumnKey)) continue;

                string colName = row[indexKeyMap.ColumnKey].ToString();
                if (string.IsNullOrEmpty(colName)) continue;
                DatabaseColumn column = new DatabaseColumn();
                column.Name = colName;
                if (!string.IsNullOrEmpty(indexKeyMap.OrdinalKey))
                {
                    int ordinal = Convert.ToInt32(row[indexKeyMap.OrdinalKey], CultureInfo.CurrentCulture);
                    column.Ordinal = ordinal;
                }
                if (ContainsColumn(c.Columns, colName)) continue;
                c.Columns.Add(column);
            }
        }

        private static bool ContainsColumn(ICollection<DatabaseColumn> columns, string columnName)
        {
            if (columns.Count == 0) return false;
            foreach (var column in columns)
            {
                if (column.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        private static bool FindBoolean(DataRowView row, string key, string trueText)
        {
            if (key == null) return false;
            var o = row[key];
            if (o == DBNull.Value) return false;
            if (o is string)
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
                DatabaseTrigger trigger = list.Find(delegate(DatabaseTrigger f) { return f.Name == name; });
                if (trigger == null)
                {
                    trigger = new DatabaseTrigger();
                    trigger.Name = name;
                    if (ownerKey != null)
                        trigger.SchemaOwner = row[ownerKey].ToString();
                    list.Add(trigger);
                }
                if (!string.IsNullOrEmpty(tableKey))
                    trigger.TableName = row[tableKey].ToString();
                if (!string.IsNullOrEmpty(bodyKey))
                    trigger.TriggerBody = row[bodyKey].ToString();
                if (!string.IsNullOrEmpty(eventKey))
                    trigger.TriggerEvent = row[eventKey].ToString();
                if (triggerTypeKey != null)
                {
                    trigger.TriggerType = row[triggerTypeKey].ToString();
                    FirebirdTriggerTypeCode(trigger);
                }
            }
            return list;
        }

        private static void FirebirdTriggerTypeCode(DatabaseTrigger trigger)
        {
            if (trigger.TriggerType.Length != 1) return;
            //firebird gives a very helpful number
            switch (trigger.TriggerType)
            {
                case "1":
                    trigger.TriggerType = "BEFORE";
                    trigger.TriggerEvent = "INSERT";
                    break;
                case "2":
                    trigger.TriggerType = "AFTER";
                    trigger.TriggerEvent = "INSERT";
                    break;
                case "3":
                    trigger.TriggerType = "BEFORE";
                    trigger.TriggerEvent = "UPDATE";
                    break;
                case "4":
                    trigger.TriggerType = "AFTER";
                    trigger.TriggerEvent = "UPDATE";
                    break;
                case "5":
                    trigger.TriggerType = "BEFORE";
                    trigger.TriggerEvent = "DELETE";
                    break;
                case "6":
                    trigger.TriggerType = "AFTER";
                    trigger.TriggerEvent = "DELETE";
                    break;
            }
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
