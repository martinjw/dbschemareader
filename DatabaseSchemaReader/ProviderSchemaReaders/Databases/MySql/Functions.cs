using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    internal class Functions : SqlExecuter<DatabaseFunction>
    {
        private readonly string _name;

        public Functions(int? commandTimeout, string owner, string name)
            : base(commandTimeout, owner)
        {
            _name = name;
            Sql = @"SELECT
  ROUTINE_SCHEMA,
  ROUTINE_NAME,
  ROUTINE_DEFINITION,
  ROUTINE_BODY,
  DATA_TYPE
FROM INFORMATION_SCHEMA.ROUTINES
WHERE 
    (ROUTINE_SCHEMA = @Owner OR (@Owner IS NULL))
    AND (ROUTINE_NAME = @Name OR (@Name IS NULL))
    AND (ROUTINE_TYPE = 'FUNCTION')
ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";

        }

        public IList<DatabaseFunction> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
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
            var sproc = new DatabaseFunction
            {
                SchemaOwner = owner,
                Name = name,
                Sql = record.GetString("ROUTINE_DEFINITION"),
                Language = record.GetString("ROUTINE_BODY"),
                ReturnType = record.GetString("DATA_TYPE"),
            };
            Result.Add(sproc);
        }
    }
}
