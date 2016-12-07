using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    internal class StoredProcedures : SqlExecuter<DatabaseStoredProcedure>
    {
        public StoredProcedures(string owner)
        {
            Owner = owner;
            Sql = @"SELECT
     rdb$owner_name AS OWNER,
     rdb$procedure_name AS PROCEDURE_NAME,
     rdb$procedure_inputs AS INPUTS,
     rdb$procedure_outputs AS OUTPUTS,
     rdb$procedure_source AS SOURCE,
     rdb$description AS DESCRIPTION
FROM rdb$procedures
WHERE
    rdb$system_flag = 0 AND
    (@Owner IS NULL OR @Owner = rdb$owner_name)
ORDER BY rdb$procedure_name
";

        }

        public IList<DatabaseStoredProcedure> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@Owner", Owner);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("OWNER");
            var name = record.GetString("PROCEDURE_NAME");
            var sproc = new DatabaseStoredProcedure
            {
                SchemaOwner = owner.TrimEnd(),
                Name = name.TrimEnd(),
                Sql = record.GetString("SOURCE"),
                Tag = record.GetString("DESCRIPTION"),
            };
            Result.Add(sproc);
        }
    }
}
