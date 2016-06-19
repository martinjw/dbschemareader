using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    internal class StoredProcedures : SqlExecuter<DatabaseStoredProcedure>
    {
        private readonly string _name;

        public StoredProcedures(string owner, string name)
        {
            _name = name;
            Owner = owner;
            Sql = @"SELECT
  ROUTINE_SCHEMA,
  ROUTINE_NAME,
  ROUTINE_DEFINITION,
  ROUTINE_BODY
FROM INFORMATION_SCHEMA.ROUTINES
WHERE 
    (ROUTINE_SCHEMA = @Owner OR (@Owner IS NULL))
    AND (ROUTINE_NAME = @Name OR (@Name IS NULL))
    AND (ROUTINE_TYPE = 'PROCEDURE')
ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";

        }

        public IList<DatabaseStoredProcedure> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@Owner", Owner);
            AddDbParameter(command, "@Name", _name);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("ROUTINE_SCHEMA");
            var name = record.GetString("ROUTINE_NAME");
            var sproc = new DatabaseStoredProcedure
            {
                SchemaOwner = owner,
                Name = name,
                Sql = record.GetString("ROUTINE_DEFINITION"),
                Language = record.GetString("ROUTINE_BODY"),
            };
            Result.Add(sproc);
        }
    }
}
