using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class MaterializedViews : OracleSqlExecuter<DatabaseView>
    {
        private readonly string _viewName;

        public MaterializedViews(string owner, string viewName)
        {
            _viewName = viewName;
            Owner = owner;
            Sql = @"SELECT
  OWNER,
  MVIEW_NAME,
  QUERY
FROM ALL_MVIEWS
WHERE (OWNER = :OWNER OR :OWNER IS NULL)
AND (MVIEW_NAME = :VIEWNAME OR :VIEWNAME IS NULL)";
        }

        public IList<DatabaseView> Execute(DbConnection connection, DbTransaction transaction)
        {
            ExecuteDbReader(connection, transaction);
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
