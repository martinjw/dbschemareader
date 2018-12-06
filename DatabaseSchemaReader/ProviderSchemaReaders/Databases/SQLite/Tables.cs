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

        public Tables(int? commandTimeout, string tableName) : base(commandTimeout, null)
        {
            _tableName = tableName;
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
            var table = new DatabaseTable
                        {
                            Name = name,
                            SchemaOwner = ""
                        };

            Result.Add(table);
        }
    }
}
