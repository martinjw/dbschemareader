using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    internal class Views : SqlExecuter<DatabaseView>
    {
        private readonly string _viewName;

        public Views(int? commandTimeout, string viewName) : base(commandTimeout, null)
        {
            _viewName = viewName;
            Sql = @"SELECT name, sql FROM sqlite_master
WHERE type='view' AND
    (name = @NAME or (@NAME is null))
ORDER BY name";
        }

        public IList<DatabaseView> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {

            AddDbParameter(command, "NAME", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var name = record["name"].ToString();
            var table = new DatabaseView
                        {
                            Name = name,
                            SchemaOwner = "",
							Sql = record.GetString("sql"),
                        };

            Result.Add(table);
        }
    }
}
