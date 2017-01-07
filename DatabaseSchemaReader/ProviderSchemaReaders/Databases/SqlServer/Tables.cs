using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
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
            Sql = @"select TABLE_SCHEMA, TABLE_NAME 
from INFORMATION_SCHEMA.TABLES 
where 
    (TABLE_SCHEMA = @Owner or (@Owner is null)) and 
    (TABLE_NAME = @TABLE_NAME or (@TABLE_NAME is null)) and 
    TABLE_TYPE = 'BASE TABLE'
 order by 
    TABLE_SCHEMA, TABLE_NAME";
        }

        public IList<DatabaseTable> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }



        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "Owner", Owner);
            AddDbParameter(command, "TABLE_NAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["TABLE_SCHEMA"].ToString();
            var name = record["TABLE_NAME"].ToString();
            var table = _factory.CreateDatabaseTable();
            table.Name = name;
            table.SchemaOwner = schema;

            Result.Add(table);
        }
    }
}
