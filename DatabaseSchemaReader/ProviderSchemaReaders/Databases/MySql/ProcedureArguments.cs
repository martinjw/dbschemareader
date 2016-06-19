using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    internal class ProcedureArguments : SqlExecuter<DatabaseArgument>
    {
        private readonly string _name;

        public ProcedureArguments(string owner, string name)
        {
            _name = name;
            Owner = owner;
            Sql = @"SELECT
  SPECIFIC_SCHEMA,
  SPECIFIC_NAME,
  PARAMETER_NAME,
  ORDINAL_POSITION,
  PARAMETER_MODE,
  DATA_TYPE,
  CHARACTER_MAXIMUM_LENGTH,
  NUMERIC_PRECISION,
  NUMERIC_SCALE,
  DATETIME_PRECISION
FROM INFORMATION_SCHEMA.PARAMETERS
WHERE (SPECIFIC_SCHEMA = @Owner OR (@Owner IS NULL))
AND (SPECIFIC_NAME = @Name OR (@Name IS NULL))
ORDER BY SPECIFIC_SCHEMA, SPECIFIC_NAME, PARAMETER_NAME";

        }

        public IList<DatabaseArgument> Execute(DbConnection connection)
        {
            try
            {
                ExecuteDbReader(connection);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Could not read parameters (older MySql does not have Information_Schema.Paramaters: " + e);
            }
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@Owner", Owner);
            AddDbParameter(command, "@Name", _name);
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
            if (inout != null) //results are null
            {
                if (inout.Contains("IN")) arg.In = true;
                //can be INOUT
                if (inout.Contains("OUT")) arg.Out = true;
            }

            Result.Add(arg);
        }
    }
}
