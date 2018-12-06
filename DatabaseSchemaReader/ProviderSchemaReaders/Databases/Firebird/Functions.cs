using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    internal class Functions : SqlExecuter<DatabaseFunction>
    {
        private readonly string _name;

        public Functions(int? commandTimeout, string name) : base(commandTimeout, null)
        {
            _name = name;
            Sql = @"SELECT
rdb$function_name AS FUNCTION_NAME,
rdb$return_argument AS RETURN_ARGUMENT
FROM rdb$functions
WHERE (rdb$function_name = @name OR @name IS NULL) AND
rdb$system_flag = 0
ORDER BY rdb$function_name
";

        }

        public IList<DatabaseFunction> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@name", _name);
        }

        protected override void Mapper(IDataRecord record)
        {
            var name = record.GetString("FUNCTION_NAME");
            var sproc = new DatabaseFunction
            {
                Name = name,
                ReturnType = record.GetString("RETURN_ARGUMENT"),
            };
            Result.Add(sproc);
        }
    }
}
