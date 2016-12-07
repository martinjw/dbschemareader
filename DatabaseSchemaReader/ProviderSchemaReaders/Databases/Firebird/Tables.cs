using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    internal class Tables : SqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;

        public Tables(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT
 rdb$relation_name AS TABLE_NAME,
 rdb$owner_name AS OWNER_NAME,
 rdb$description AS DESCRIPTION
FROM rdb$relations
WHERE
  rdb$view_source IS NULL AND 
  rdb$system_flag = 0 AND
  (@Owner IS NULL OR @Owner = rdb$owner_name) AND
  (@TABLE_NAME IS NULL OR @TABLE_NAME = rdb$relation_name)
ORDER BY 
  rdb$owner_name, rdb$relation_name
";
        }

        public IList<DatabaseTable> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@Owner", Owner);
            AddDbParameter(command, "@TABLE_NAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            //Firebird doesn't have schemas, but it does have owners
            var schema = record["OWNER_NAME"].ToString();
            var name = record["TABLE_NAME"].ToString();
            var table = new DatabaseTable
                        {
                            Name = name.Trim(),
                            SchemaOwner = schema.Trim(),
                            Description = record.GetString("DESCRIPTION"),
                        };

            Result.Add(table);
        }
    }
}
