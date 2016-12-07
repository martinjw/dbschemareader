using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    internal class ProcedureArguments : SqlExecuter<DatabaseArgument>
    {
        public ProcedureArguments(string owner)
        {
            Owner = owner;
            Sql = @"SELECT
     fld.rdb$owner_name AS OWNER,
     pp.rdb$procedure_name AS PROCEDURE_NAME,
     pp.rdb$parameter_name AS PARAMETER_NAME,
     fld.rdb$field_type AS FIELD_TYPE,
     CASE fld.rdb$field_type
          WHEN 261 THEN 'BLOB'
          WHEN 14 THEN 'CHAR'
          WHEN 40 THEN 'CSTRING'
          WHEN 11 THEN 'D_FLOAT'
          WHEN 27 THEN 'DOUBLE'
          WHEN 10 THEN 'FLOAT'
          WHEN 16 THEN 'INT64'
          WHEN 8 THEN 'INTEGER'
          WHEN 9 THEN 'QUAD'
          WHEN 7 THEN 'SMALLINT'
          WHEN 12 THEN 'DATE'
          WHEN 13 THEN 'TIME'
          WHEN 35 THEN 'TIMESTAMP'
          WHEN 37 THEN 'VARCHAR'
          ELSE ''
        END AS DATA_TYPE,
     fld.rdb$field_sub_type AS PARAMETER_SUB_TYPE,
     pp.rdb$parameter_number AS ORDINAL_POSITION,
     CAST(pp.rdb$parameter_type AS integer) AS PARAMETER_DIRECTION,
     CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
     CAST(fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
     CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
     pp.rdb$description AS DESCRIPTION
FROM rdb$procedure_parameters pp
     LEFT JOIN rdb$fields fld ON pp.rdb$field_source = fld.rdb$field_name
WHERE
    pp.rdb$system_flag = 0 AND 
    (@Owner IS NULL OR @Owner = fld.rdb$owner_name)
ORDER BY pp.rdb$procedure_name, pp.rdb$parameter_type, pp.rdb$parameter_number
";

        }

        public IList<DatabaseArgument> Execute(DbConnection connection)
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
            var owner = record.GetString("OWNER").TrimEnd();
            var sprocName = record.GetString("PROCEDURE_NAME").TrimEnd();
            var name = record.GetString("PARAMETER_NAME").TrimEnd();
            var arg = new DatabaseArgument
            {
                SchemaOwner = owner,
                ProcedureName = sprocName,
                Name = name,
                Ordinal = record.GetNullableInt("ORDINAL_POSITION").GetValueOrDefault(),
                DatabaseDataType = record.GetString("DATA_TYPE").TrimEnd(),
                Length = record.GetNullableInt("CHARACTER_MAX_LENGTH"),
                Precision = record.GetNullableInt("NUMERIC_PRECISION"),
                Scale = record.GetNullableInt("NUMERIC_SCALE"),
            };
            //Indicates whether the parameter is for input (value 0) or output (value 1)
            var inout = record.GetNullableInt("PARAMETER_DIRECTION");
            if (inout == 1)
            {
                arg.Out = true;
            }
            else
            {
                arg.In = true;
            }

            Result.Add(arg);
        }
    }
}
