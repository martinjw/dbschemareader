using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class Triggers : OracleSqlExecuter<DatabaseTrigger>
    {
        private readonly string _tableName;
        public Triggers(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;

            Sql = @"SELECT OWNER,
  TRIGGER_NAME,
  TABLE_NAME,
  TRIGGER_BODY,
  TRIGGERING_EVENT,
  TRIGGER_TYPE
FROM ALL_TRIGGERS
WHERE STATUS = 'ENABLED' AND 
(TABLE_NAME = :tableName OR :tableName IS NULL) AND 
(OWNER = :schemaOwner OR :schemaOwner IS NULL) AND 
TRIGGER_NAME NOT IN ( SELECT object_name FROM USER_RECYCLEBIN ) 
";

        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
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

        public IList<DatabaseTrigger> Execute(DbConnection dbConnection, DbTransaction transaction)
        {
            ExecuteDbReader(dbConnection, transaction);
            return Result;
        }
    }
}
