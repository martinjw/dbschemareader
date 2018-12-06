using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    class Triggers : SqlExecuter<DatabaseTrigger>
    {
        private readonly string _tableName;
        public Triggers(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;

            Sql = @"SELECT
 tr.name AS TRIGGER_NAME,
 SCHEMA_NAME(parent.schema_id) AS TRIGGER_SCHEMA,
 SCHEMA_NAME(parent.schema_id) AS TABLE_SCHEMA,
 parent.name AS TABLE_NAME,
 OBJECTPROPERTY(tr.object_id, 'ExecIsUpdateTrigger') AS IS_UPDATE,
 OBJECTPROPERTY(tr.object_id, 'ExecIsDeleteTrigger') AS IS_DELETE,
 OBJECTPROPERTY(tr.object_id, 'ExecIsInsertTrigger') AS IS_INSERT,
 OBJECTPROPERTY(tr.object_id, 'ExecIsAfterTrigger') AS IS_AFTER,
 tr.is_instead_of_trigger AS IS_INSTEADOF,
 tr.is_disabled AS IS_DISABLED,
 OBJECT_DEFINITION(tr.object_id) AS TRIGGER_BODY
FROM sys.triggers AS tr
 INNER JOIN sys.tables AS parent
  ON tr.parent_id = parent.object_id
WHERE (SCHEMA_NAME(parent.schema_id) = @Owner or (@Owner is null)) 
    AND (parent.name = @TABLE_NAME or (@TABLE_NAME is null)) 
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

        public IList<DatabaseTrigger> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
