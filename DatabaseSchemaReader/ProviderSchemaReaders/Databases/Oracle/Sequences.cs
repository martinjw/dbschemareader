using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class Sequences : OracleSqlExecuter<DatabaseSequence>
    {

        public Sequences(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"
SELECT
  SEQUENCE_OWNER,
  SEQUENCE_NAME,
  MIN_VALUE,
  INCREMENT_BY,
  CYCLE_FLAG
FROM ALL_SEQUENCES
WHERE  
(SEQUENCE_OWNER = :schemaOwner OR :schemaOwner IS NULL)";

        }

        public IList<DatabaseSequence> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "schemaOwner", Owner);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("SEQUENCE_OWNER");
            var name = record.GetString("SEQUENCE_NAME");
            var sproc = new DatabaseSequence
            {
                SchemaOwner = owner,
                Name = name,
                MinimumValue = record.GetNullableInt("MIN_VALUE"),
                IncrementBy = record.GetNullableInt("INCREMENT_BY").GetValueOrDefault(),
            };
            Result.Add(sproc);
        }
    }
}
