using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    class Triggers : SqlExecuter<DatabaseTrigger>
    {
        private readonly string _tableName;
        public Triggers(int? commandTimeout, string owner, string tableName)
            : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;

            Sql = @"SELECT 
  TRIGGER_SCHEMA AS 'OWNER',
  TRIGGER_NAME,
  EVENT_OBJECT_TABLE AS 'TABLE_NAME',
  ACTION_STATEMENT AS 'TRIGGER_BODY',
  EVENT_MANIPULATION AS 'TRIGGERING_EVENT',
  ACTION_TIMING AS 'TRIGGER_TYPE'
FROM information_schema.Triggers
WHERE 
(EVENT_OBJECT_TABLE = @tableName OR @tableName IS NULL) AND 
(TRIGGER_SCHEMA = @schemaOwner OR @schemaOwner IS NULL)
";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@schemaOwner", Owner);
            AddDbParameter(command, "@tableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var trigger = new DatabaseTrigger
            {
                Name = record.GetString("TRIGGER_NAME"),
                SchemaOwner = record.GetString("OWNER"),
                TableName = record.GetString("TABLE_NAME"),
                TriggerBody = record.GetString("TRIGGER_BODY"),
                TriggerEvent = record.GetString("TRIGGERING_EVENT"),
                TriggerType = record.GetString("TRIGGER_TYPE"),
            };

            Result.Add(trigger);
        }

        public IList<DatabaseTrigger> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
