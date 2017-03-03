using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Views : SqlExecuter<DatabaseView>
    {
        private readonly string _viewName;

        public Views(string owner, string viewName)
        {
            _viewName = viewName;
            Owner = owner;
            Sql = @"SELECT
  table_schema,
  table_name,
  is_updatable
FROM information_schema.views
WHERE (table_schema = :OWNER OR :OWNER IS NULL)
AND (table_name = :TABLENAME OR :TABLENAME IS NULL)
ORDER BY table_schema, table_name";
        }

        public IList<DatabaseView> Execute(DbConnection connection, DbTransaction transaction)
        {
            ExecuteDbReader(connection, transaction);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "OWNER", Owner);
            AddDbParameter(command, "TABLENAME", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["table_schema"].ToString();
            var name = record["table_name"].ToString();
            var table = new DatabaseView
            {
                Name = name,
                SchemaOwner = schema
            };

            Result.Add(table);
        }
    }
}