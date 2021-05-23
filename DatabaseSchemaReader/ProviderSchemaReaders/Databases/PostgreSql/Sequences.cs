using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Sequences : SqlExecuter<DatabaseSequence>
    {
        private string Sql10;

        public Sequences(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Owner = owner;
            //based on http://www.alberton.info/postgresql_meta_info.html before pg_sequences existed
            Sql = @"SELECT 
ns.nspname AS schemaname, 
c.relname as sequencename,
NULL as start_value,
NULL as increment_by,
NULL as max_value
FROM pg_class c
INNER JOIN pg_namespace ns ON c.relnamespace = ns.oid
WHERE c.relkind = 'S'
AND c.relnamespace IN (
        SELECT oid
          FROM pg_namespace
         WHERE nspname NOT LIKE 'pg_%'
           AND nspname != 'information_schema'
)";
            Sql10 = @"SELECT
schemaname, sequencename, start_value, increment_by, max_value
FROM pg_sequences";
        }

        public IList<DatabaseSequence> Execute(IConnectionAdapter connectionAdapter, int serverVersion)
        {
            try
            {
                if (serverVersion >= 100000)
                {
                    Sql = Sql10; //use pgSequences
                }
                ExecuteDbReader(connectionAdapter);
            }
            catch (DbException ex)
            {
                System.Diagnostics.Trace.WriteLine("Error reading PostgreSql sequences " + ex.Message);
            }
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("schemaname");
            var name = record.GetString("sequencename");
            //these are actually bigInts, but they are likely to be ints
            var increment = record.GetNullableInt("increment_by");
            var min = record.GetNullableLong("start_value");
            var max = record.GetNullableLong("max_value");

            var sequence = new DatabaseSequence
            {
                SchemaOwner = owner,
                Name = name,
                IncrementBy = increment ?? 0,
                MinimumValue = min,
                MaximumValue = max
            };
            Result.Add(sequence);
        }
    }
}