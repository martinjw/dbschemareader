using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class MaterializedViews : OracleSqlExecuter<DatabaseView>
    {
        private readonly string _viewName;

        public MaterializedViews(int? commandTimeout, string owner, string viewName) : base(commandTimeout, owner)
        {
            _viewName = viewName;
            Sql = @"SELECT
  OWNER,
  MVIEW_NAME,
  QUERY
FROM ALL_MVIEWS
WHERE (OWNER = :OWNER OR :OWNER IS NULL)
AND (MVIEW_NAME = :VIEWNAME OR :VIEWNAME IS NULL)";
        }

        public IList<DatabaseView> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }



        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "OWNER", Owner);
            AddDbParameter(command, "VIEWNAME", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["OWNER"].ToString();
            var name = record["MVIEW_NAME"].ToString();
            var table = new DatabaseView
                        {
                            Name = name,
                            SchemaOwner = schema,
                            Sql = record.GetString("QUERY"),
                            Tag = "Materialized View"
                        };

            Result.Add(table);
        }
    }
}
