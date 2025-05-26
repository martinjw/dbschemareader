using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    class Triggers : SqlExecuter<DatabaseTrigger>
    {
        private readonly string _tableName;
        public Triggers(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "tableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var trigger = new DatabaseTrigger
            {
                Name = record.GetString("TRIGGER_NAME"),
                SchemaOwner = record.GetString("OWNER"),
                TableName = record.GetString("TABLE_NAME"),
                TriggerBody = record.GetString("TRIGGER_BODY"),
                TriggerType = record.GetString("TRIGGER_TYPE"),
                TriggerEvent = record.GetString("TRIGGERING_EVENT"),
            };
            Result.Add(trigger);
        }
        public int ServerVersion { get; set; }

        public IList<DatabaseTrigger> Execute(IConnectionAdapter connectionAdapter)
        {
            string timing = "CONDITION_TIMING";
            if (ServerVersion >= 90100)
            {
                timing = "ACTION_TIMING";
            }

//            Sql = $@"SELECT 
//  TRIGGER_SCHEMA AS OWNER,
//  TRIGGER_NAME,
//  EVENT_OBJECT_TABLE AS TABLE_NAME,
//  ACTION_STATEMENT AS TRIGGER_BODY,
//  EVENT_MANIPULATION AS TRIGGERING_EVENT,
//  {timing} AS TRIGGER_TYPE
//FROM information_schema.Triggers
//WHERE 
//(EVENT_OBJECT_TABLE = :tableName OR :tableName IS NULL) AND 
//(TRIGGER_SCHEMA = :schemaOwner OR :schemaOwner IS NULL)";
            Sql = $@"SELECT 
  it.trigger_schema AS OWNER,
  it.trigger_name AS TRIGGER_NAME,
  it.event_object_table AS TABLE_NAME,
  pg_get_triggerdef(t.oid) AS TRIGGER_BODY,
  it.event_manipulation AS TRIGGERING_EVENT,
  it.{timing} AS TRIGGER_TYPE
FROM pg_trigger t
JOIN pg_class c ON t.tgrelid = c.oid
JOIN pg_namespace n ON c.relnamespace = n.oid
JOIN information_schema.triggers it 
  ON it.trigger_name = t.tgname 
  AND it.event_object_table = c.relname
  AND it.trigger_schema = n.nspname
WHERE NOT t.tgisinternal
AND (it.EVENT_OBJECT_TABLE = :tableName OR :tableName IS NULL) AND 
(it.TRIGGER_SCHEMA = :schemaOwner OR :schemaOwner IS NULL)";

            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
