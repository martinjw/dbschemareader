using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    class Triggers : SqlExecuter<DatabaseTrigger>
    {
        private readonly string _tableName;
        public Triggers(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;

            Sql = @"SELECT
  so.name AS TRIGGER_NAME,
  USER_NAME(so.uid) AS TRIGGER_SCHEMA,
  USER_NAME(parent.uid) AS TABLE_SCHEMA,
  OBJECT_NAME(so.parent_obj) AS TABLE_NAME,
  OBJECTPROPERTY(so.id, 'ExecIsUpdateTrigger') AS IS_UPDATE,
  OBJECTPROPERTY(so.id, 'ExecIsDeleteTrigger') AS IS_DELETE,
  OBJECTPROPERTY(so.id, 'ExecIsInsertTrigger') AS IS_INSERT,
  OBJECTPROPERTY(so.id, 'ExecIsAfterTrigger') AS IS_AFTER,
  OBJECTPROPERTY(so.id, 'ExecIsInsteadOfTrigger') AS IS_INSTEADOF,
  OBJECTPROPERTY(so.id, 'ExecIsTriggerDisabled') AS IS_DISABLED,
  OBJECT_DEFINITION(so.id) AS TRIGGER_BODY
FROM sysobjects AS so
INNER JOIN sysobjects AS parent
  ON so.parent_obj = parent.Id
WHERE so.type = 'TR'
    AND (USER_NAME(parent.uid) = @Owner or (@Owner is null)) 
    AND (OBJECT_NAME(so.parent_obj) = @TABLE_NAME or (@TABLE_NAME is null)) 
";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "Owner", Owner);
            AddDbParameter(command, "TABLE_NAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var trigger = new DatabaseTrigger
            {
                Name = record.GetString("TRIGGER_NAME"),
                SchemaOwner = record.GetString("TABLE_SCHEMA"),
                TableName = record.GetString("TABLE_NAME"),
                TriggerBody = record.GetString("TRIGGER_BODY"),
            };
            var triggerEvents = new List<string>();
            if (record.GetBoolean("IS_UPDATE"))
            {
                triggerEvents.Add("UPDATE");
            }
            if (record.GetBoolean("IS_DELETE"))
            {
                triggerEvents.Add("DELETE");
            }
            if (record.GetBoolean("IS_INSERT"))
            {
                triggerEvents.Add("INSERT");
            }
            trigger.TriggerEvent = string.Join(",", triggerEvents.ToArray());
            if (record.GetBoolean("IS_AFTER"))
            {
                trigger.TriggerType = "AFTER";
            }
            if (record.GetBoolean("IS_INSTEADOF"))
            {
                trigger.TriggerType = "INSTEAD OF";
            }
            Result.Add(trigger);
        }

        public IList<DatabaseTrigger> Execute(DbConnection dbConnection)
        {
            ExecuteDbReader(dbConnection);
            return Result;
        }
    }
}
