using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    internal class Views : SqlExecuter<DatabaseView>
    {
        private readonly string _viewName;

        public Views(string owner, string viewName)
        {
            _viewName = viewName;
            Owner = owner;
            Sql = @"SELECT
 rdb$relation_name AS TABLE_NAME,
 rdb$owner_name AS OWNER_NAME,
 rdb$description AS DESCRIPTION,
 rdb$view_source AS VIEW_SOURCE
FROM rdb$relations
WHERE
  rdb$view_source IS NOT NULL AND 
  (@Owner IS NULL OR @Owner = rdb$owner_name) AND
  (@TABLE_NAME IS NULL OR @TABLE_NAME = rdb$relation_name)
ORDER BY 
  rdb$owner_name, rdb$relation_name
";
        }

        public IList<DatabaseView> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@Owner", Owner);
            AddDbParameter(command, "@TABLE_NAME", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["OWNER_NAME"].ToString();
            var name = record["TABLE_NAME"].ToString();
            var table = new DatabaseView
                        {
                            Name = name.Trim(),
                            SchemaOwner = schema.Trim(),
                            Sql = record.GetString("VIEW_SOURCE"),
                            Description = record.GetString("DESCRIPTION"),
                        };

            Result.Add(table);
        }
    }
}
