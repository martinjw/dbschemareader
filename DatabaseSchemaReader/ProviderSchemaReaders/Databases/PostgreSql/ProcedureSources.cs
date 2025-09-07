using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class ProcedureSources : SqlExecuter<ProcedureSource>
    {
        private readonly string _name;

        public ProcedureSources(int? commandTimeout, string owner, string name) : base(commandTimeout, owner)
        {
            _name = name;
            Owner = owner;
            Sql = @"SELECT
    ns.nspname AS Owner,
    p.proname AS Name,
    p.prokind AS Type,
    pg_get_functiondef(p.oid) AS Text,
    p.oid AS OID
FROM
    pg_proc p
JOIN
    pg_namespace ns ON p.pronamespace = ns.oid
WHERE
  (p.proname = :name OR :name IS NULL)
  AND p.prokind != 'a'
  AND ns.nspname NOT LIKE 'pg_%'
  AND ns.nspname != 'information_schema'
  AND (ns.nspname = :schemaOwner OR :schemaOwner IS NULL);";
            //NB: pg_get_functiondef does not support aggregate functions - errors with "array_agg" is an aggregate function
        }

        public IList<ProcedureSource> Execute(IConnectionAdapter connectionAdapter)
        {
            try
            {
                ExecuteDbReader(connectionAdapter);
            }
            catch (DbException exception)
            {
                //1. Security does not allow access
                Trace.TraceError("Handled: " + exception);
                //continue without the source
            }
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "name", _name);
        }

        protected override void Mapper(IDataRecord record)
        {
            var source = new ProcedureSource
            {
                Name = record.GetString("Name"),
                SchemaOwner = record.GetString("Owner")
            };
            source.Oid = (uint)record["oid"];
            var type = record.GetString("Type").Trim();
            switch (type)
            {
                case "p": //procedure
                    source.SourceType = SourceType.StoredProcedure;
                    break;

                case "f": //function
                case "a": //aggregate
                    source.SourceType = SourceType.Function;
                    break;
            }
            source.Text = record.GetString("Text");
            Result.Add(source);
        }
    }
}