using System;
using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class MaterializedViews : SqlExecuter<DatabaseView>
    {
        private readonly string _viewName;
        private readonly string _sql93;

        public MaterializedViews(int? commandTimeout, string owner, string viewName) : base(commandTimeout, owner)
        {
            _viewName = viewName;
            Owner = owner;
            Sql = @"SELECT
  ns.nspname AS schemaname,
  mv.relname AS matviewname,
  NULL AS definition
FROM pg_class mv
    JOIN pg_namespace ns ON mv.relnamespace = ns.oid
WHERE mv.relkind = 'm'
AND (ns.nspname = :OWNER OR :OWNER IS NULL)
AND (mv.relname = :TABLENAME OR :TABLENAME IS NULL)
ORDER BY ns.nspname, mv.relname";
            //for 9.3 +
            _sql93 = @"SELECT
schemaname,
matviewname,
definition
FROM pg_matviews
WHERE (schemaname = :OWNER OR :OWNER IS NULL)
AND (matviewname = :TABLENAME OR :TABLENAME IS NULL)
ORDER BY schemaname, matviewname";
        }

        public int ServerVersion { get; set; }

        public IList<DatabaseView> Execute(IConnectionAdapter connectionAdapter)
        {
            //or is there something on connection?
            try
            {
                var hasMatViewsTable = ServerVersion > 90300;
                if (hasMatViewsTable)
                {
                    Sql = _sql93;
                }
                ExecuteDbReader(connectionAdapter);
            }
            catch (Exception exception)
            {
                //possibly older than 8.2
                Trace.TraceError("Error reading postgresql materialized views " + exception);
            }
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "OWNER", Owner);
            AddDbParameter(command, "TABLENAME", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["schemaname"].ToString();
            var name = record["matviewname"].ToString();
            var table = new DatabaseView
            {
                Name = name,
                SchemaOwner = schema,
                Sql = record.GetString("definition"),
                Tag = "Materialized View"
            };

            Result.Add(table);
        }
    }
}