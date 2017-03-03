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
        public Triggers(string owner, string tableName)
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

        public IList<DatabaseTrigger> Execute(IConnectionAdapter connectionAdapter)
        {
            var version = connectionAdapter.DbConnection.ServerVersion;
            string timing = "CONDITION_TIMING";
            if (version != null)
            {
                if (version.IndexOf(',') != -1)
                {
                    //Devart shows an unparseable string
                    //PostgreSQL 9.3.4, compiled by Visual C++ build 1600, 64-bit 
                    version = version.Substring(0, version.IndexOf(','))
                        .Replace("PostgreSQL ", "");
                }
                var v = new Version(version);
                if (v.Major >= 9 && v.Minor >= 1)
                {
                    timing = "ACTION_TIMING";
                }
            }

            Sql = @"SELECT 
  TRIGGER_SCHEMA AS OWNER,
  TRIGGER_NAME,
  EVENT_OBJECT_TABLE AS TABLE_NAME,
  ACTION_STATEMENT AS TRIGGER_BODY,
  EVENT_MANIPULATION AS TRIGGERING_EVENT,
  " + timing + @" AS TRIGGER_TYPE
FROM information_schema.Triggers
WHERE 
(EVENT_OBJECT_TABLE = :tableName OR :tableName IS NULL) AND 
(TRIGGER_SCHEMA = :schemaOwner OR :schemaOwner IS NULL)";

            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
