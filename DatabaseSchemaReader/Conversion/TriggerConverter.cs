using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
{
    /// <summary>
    /// Converts the "Triggers" DataTable into <see cref="DatabaseTrigger"/> objects
    /// </summary>
    class TriggerConverter
    {
        private readonly IList<DatabaseTrigger> _triggers;

        public TriggerConverter(DataTable dt)
        {
            _triggers = Triggers(dt, null);
        }

        public IEnumerable<DatabaseTrigger> Triggers(string tableName)
        {
            return _triggers.Where(x => x.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Converts the "Triggers" DataTable into <see cref="DatabaseTrigger"/> objects
        /// </summary>
        private static List<DatabaseTrigger> Triggers(DataTable dt, string tableName)
        {
            var list = new List<DatabaseTrigger>();
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
            if (!String.IsNullOrEmpty(tableName) && !String.IsNullOrEmpty(tableKey))
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
                if (!String.IsNullOrEmpty(tableKey))
                    trigger.TableName = row[tableKey].ToString();
                if (!String.IsNullOrEmpty(bodyKey))
                    trigger.TriggerBody = row[bodyKey].ToString();
                if (!String.IsNullOrEmpty(eventKey))
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
    }
}
