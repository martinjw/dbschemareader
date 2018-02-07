using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    internal class Tables : SqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;
        private readonly SchemaFactory _factory;

        public Tables(string tableName, SchemaFactory factory)
        {
            _tableName = tableName;
            _factory = factory;
            Sql = @"SELECT name FROM sqlite_master
WHERE type='table' AND
    (name = @TABLE_NAME or (@TABLE_NAME is null))
ORDER BY name";
        }

        public IList<DatabaseTable> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }



        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "TABLE_NAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var name = record["name"].ToString();
            var table = _factory.CreateDatabaseTable();
            table.Name = name;
            table.SchemaOwner = string.Empty;

            Result.Add(table);
        }
    }
}
