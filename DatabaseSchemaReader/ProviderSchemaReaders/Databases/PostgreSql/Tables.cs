using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Tables : SqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;
        private readonly SchemaFactory _factory;

        public Tables(string owner, string tableName, SchemaFactory factory)
        {
            _tableName = tableName;
            _factory = factory;
            Owner = owner;
            Sql = @"SELECT 
table_schema, 
table_name 
FROM information_schema.tables 
WHERE (table_schema = :OWNER OR :OWNER IS NULL)
AND (table_name = :TABLENAME OR :TABLENAME IS NULL)
ORDER BY table_schema, table_name";
        }

        public IList<DatabaseTable> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }



        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "OWNER", Owner);
            AddDbParameter(command, "TABLENAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["table_schema"].ToString();
            var name = record["table_name"].ToString();
            var table = _factory.CreateDatabaseTable();
            table.Name = name;
            table.SchemaOwner = schema;

            Result.Add(table);
        }
    }
}
