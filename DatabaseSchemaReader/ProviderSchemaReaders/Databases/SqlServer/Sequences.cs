using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    class Sequences : SqlExecuter<DatabaseSequence>
    {

        public Sequences(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Owner = owner;
            Sql = @"
SELECT Schema_name(schema_id) AS sequence_owner,
       name                   AS sequence_name,
       start_value            AS min_value,
       increment              AS increment_by,
       is_cycling             AS cycle_flag
FROM   sys.sequences
WHERE  
(Schema_name(schema_id) = @schemaOwner OR @schemaOwner IS NULL)";

        }

        public IList<DatabaseSequence> Execute(IConnectionAdapter connectionAdapter)
        {
            var cmd = BuildCommand(connectionAdapter);
            //step 1- check if there are any sequences (backwards compatible)
            cmd.CommandText = @"SELECT COUNT(*) 
FROM sys.objects 
WHERE type= 'SO' AND
(Schema_name(schema_id) = @schemaOwner OR @schemaOwner IS NULL)";
            AddDbParameter(cmd, "schemaOwner", Owner);
            var hasSequences = (int)cmd.ExecuteScalar() > 0;
            if (!hasSequences)
            {
                return new List<DatabaseSequence>();
            }
            //step 2- they have them
            //we can use the SqlServer 2012 sys.sequences catalog view
            //renamed for compatibility with Oracle's ALL_SEQUENCES
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("sequence_owner");
            var name = record.GetString("sequence_name");
            var sproc = new DatabaseSequence
            {
                SchemaOwner = owner,
                Name = name,
                MinimumValue = record.GetNullableInt("min_value"),
                IncrementBy = record.GetNullableInt("increment_by").GetValueOrDefault(),
            };
            Result.Add(sproc);
        }
    }
}
