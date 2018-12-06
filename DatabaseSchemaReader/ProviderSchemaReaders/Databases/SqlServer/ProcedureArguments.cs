using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    internal class ProcedureArguments : SqlExecuter<DatabaseArgument>
    {
        private readonly string _name;

        public ProcedureArguments(int? commandTimeout, string owner, string name) : base(commandTimeout, owner)
        {
            _name = name;
            Owner = owner;
            Sql = @"SELECT
  SPECIFIC_SCHEMA,
  SPECIFIC_NAME,
  PARAMETER_NAME,
  ORDINAL_POSITION,
  PARAMETER_MODE,
  IS_RESULT,
  AS_LOCATOR,
  CASE
    WHEN DATA_TYPE IS NULL THEN USER_DEFINED_TYPE_NAME
    WHEN DATA_TYPE = 'table type' THEN USER_DEFINED_TYPE_NAME
    ELSE DATA_TYPE
  END AS DATA_TYPE,
  CHARACTER_MAXIMUM_LENGTH,
  CHARACTER_OCTET_LENGTH,
  COLLATION_CATALOG,
  COLLATION_SCHEMA,
  COLLATION_NAME,
  CHARACTER_SET_CATALOG,
  CHARACTER_SET_SCHEMA,
  CHARACTER_SET_NAME,
  NUMERIC_PRECISION,
  NUMERIC_PRECISION_RADIX,
  NUMERIC_SCALE,
  DATETIME_PRECISION,
  INTERVAL_TYPE,
  INTERVAL_PRECISION
FROM INFORMATION_SCHEMA.PARAMETERS
WHERE (SPECIFIC_SCHEMA = @Owner OR (@Owner IS NULL))
AND (SPECIFIC_NAME = @Name OR (@Name IS NULL))
ORDER BY SPECIFIC_SCHEMA, SPECIFIC_NAME, PARAMETER_NAME";

        }

        public IList<DatabaseArgument> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "Owner", Owner);
            AddDbParameter(command, "Name", _name);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("SPECIFIC_SCHEMA");
            var sprocName = record.GetString("SPECIFIC_NAME");
            var name = record.GetString("PARAMETER_NAME");
            var arg = new DatabaseArgument
            {
                SchemaOwner = owner,
                ProcedureName = sprocName,
                Name = name,
                Ordinal = record.GetNullableInt("ORDINAL_POSITION").GetValueOrDefault(),
                DatabaseDataType = record.GetString("DATA_TYPE"),
                Length = record.GetNullableInt("CHARACTER_MAXIMUM_LENGTH"),
                Precision = record.GetNullableInt("NUMERIC_PRECISION"),
                Scale = record.GetNullableInt("NUMERIC_SCALE"),
            };
            string inout = record.GetString("PARAMETER_MODE");
            if (inout.Contains("IN")) arg.In = true;
            //can be INOUT
            if (inout.Contains("OUT")) arg.Out = true;

            Result.Add(arg);
        }
    }
}
