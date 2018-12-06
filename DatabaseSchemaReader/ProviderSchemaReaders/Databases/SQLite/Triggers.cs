using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    class Triggers : SqlExecuter<DatabaseTrigger>
    {
        private readonly string _tableName;
        public Triggers(int? commandTimeout, string tableName) : base(commandTimeout, null)
        {
            _tableName = tableName;

            Sql = @"SELECT name, tbl_name, sql FROM sqlite_master
WHERE type='trigger' AND
    (tbl_name = @TABLE_NAME or (@TABLE_NAME is null))
ORDER BY tbl_name, name";

        }

        protected override void AddParameters(DbCommand command)
        {

            AddDbParameter(command, "TABLE_NAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var trigger = new DatabaseTrigger
            {
                Name = record.GetString("name"),
                SchemaOwner = "",
                TableName = record.GetString("tbl_name"),
                TriggerBody = record.GetString("sql"),
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
