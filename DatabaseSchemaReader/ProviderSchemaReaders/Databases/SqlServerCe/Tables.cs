using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServerCe
{
    internal class Tables : SqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;

        public Tables(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"select TABLE_SCHEMA, TABLE_NAME, DESCRIPTION 
from INFORMATION_SCHEMA.TABLES 
where 
    (TABLE_SCHEMA = @Owner or (@Owner is null)) and 
    (TABLE_NAME = @TABLE_NAME or (@TABLE_NAME is null)) and 
    TABLE_TYPE = 'TABLE'
 order by 
    TABLE_SCHEMA, TABLE_NAME";
        }

        public IList<DatabaseTable> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }



        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "Owner", Owner, DbType.String);
            AddDbParameter(command, "TABLE_NAME", _tableName, DbType.String);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("TABLE_SCHEMA");
            var name = record["TABLE_NAME"].ToString();
            var table = new DatabaseTable
                        {
                            Name = name,
                            SchemaOwner = schema,
                            Description = record.GetString("DESCRIPTION"),
                        };

            Result.Add(table);
        }
    }
}
