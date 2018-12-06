using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Functions : SqlExecuter<DatabaseFunction>
    {
        public Functions(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Owner = owner;
            //Npgsql doesn't have a functions collection, so this is a simple substitute
            //based on http://www.alberton.info/postgresql_meta_info.html 
            Sql = @"SELECT 
ns.nspname AS SCHEMA, 
pr.proname AS NAME, 
tp.typname AS RETURNTYPE, 
lng.lanname AS LANGUAGE,
pr.prosrc AS BODY
  FROM pg_proc pr
LEFT OUTER JOIN pg_type tp ON tp.oid = pr.prorettype
INNER JOIN pg_namespace ns ON pr.pronamespace = ns.oid
INNER JOIN pg_language lng ON lng.oid = pr.prolang
 WHERE --pr.proisagg = FALSE AND 
  tp.typname <> 'trigger'
  AND ns.nspname NOT LIKE 'pg_%'
  AND ns.nspname != 'information_schema'
  AND (ns.nspname = :schemaOwner OR :schemaOwner IS NULL)
 ORDER BY pr.proname";

        }

        public IList<DatabaseFunction> Execute(IConnectionAdapter connectionAdapter)
        {
            try
            {
                ExecuteDbReader(connectionAdapter);
            }
            catch (DbException ex)
            {
                System.Diagnostics.Trace.WriteLine("Error reading PostgreSql functions " + ex.Message);
            }
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("SCHEMA");
            var name = record.GetString("NAME");
            var sql = record.GetString("BODY");
            var sproc = new DatabaseFunction
            {
                SchemaOwner = owner,
                Name = name,
                Sql = sql,
                Language = record.GetString("LANGUAGE"),
                ReturnType = record.GetString("RETURNTYPE")
            };
            Result.Add(sproc);
        }
    }
}
