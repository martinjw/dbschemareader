using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    class Sequences : SqlExecuter<DatabaseSequence>
    {

        public Sequences()
        {
            //"Generators" in Firebird
            Sql = @"
SELECT RDB$GENERATOR_NAME AS SEQUENCE_NAME
FROM RDB$GENERATORS
WHERE RDB$SYSTEM_FLAG=0";

        }

        public IList<DatabaseSequence> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
        }

        protected override void Mapper(IDataRecord record)
        {
            var name = record.GetString("SEQUENCE_NAME").Trim();
            var sequence = new DatabaseSequence
            {
                Name = name,
            };
            Result.Add(sequence);
        }
    }
}
