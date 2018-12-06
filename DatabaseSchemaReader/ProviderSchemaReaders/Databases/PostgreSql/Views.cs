using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Views : SqlExecuter<DatabaseView>
    {
        private readonly string _viewName;

        public Views(int? commandTimeout, string owner, string viewName) : base(commandTimeout, owner)
        {
            _viewName = viewName;
            Owner = owner;
            Sql = @"SELECT
  table_schema,
  table_name,
  is_updatable,
  view_definition
FROM information_schema.views
WHERE (table_schema = :OWNER OR :OWNER IS NULL)
AND (table_name = :TABLENAME OR :TABLENAME IS NULL)
ORDER BY table_schema, table_name";
        }

        public IList<DatabaseView> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
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
            var sql = record.GetString("view_definition");
            var table = new DatabaseView
            {
                Name = name,
                SchemaOwner = schema,
                Sql = sql
            };

            Result.Add(table);
        }
    }
}