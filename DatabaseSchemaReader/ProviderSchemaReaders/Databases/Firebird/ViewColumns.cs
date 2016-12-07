using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    internal class ViewColumns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _viewName;

        public ViewColumns(string owner, string viewName)
        {
            _viewName = viewName;
            Owner = owner;
            Sql = @"SELECT
     rel.rdb$owner_name AS OWNER_NAME,
     rel.rdb$relation_name AS VIEW_NAME,
     rfr.rdb$field_name AS COLUMN_NAME,
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
     fld.rdb$field_sub_type AS COLUMN_SUB_TYPE,
     CAST(fld.rdb$field_length AS integer) AS COLUMN_SIZE,
     CAST(fld.rdb$field_precision AS integer) AS NUMERIC_PRECISION,
     CAST(fld.rdb$field_scale AS integer) AS NUMERIC_SCALE,
     CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAX_LENGTH,
     CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
     rfr.rdb$field_position AS ORDINAL_POSITION,
     fld.rdb$default_source AS COLUMN_DEFAULT,
     coalesce(fld.rdb$null_flag, rfr.rdb$null_flag) AS IS_NULLABLE,
     rfr.rdb$description AS DESCRIPTION
FROM rdb$relations rel
     LEFT JOIN rdb$relation_fields rfr ON rel.rdb$relation_name = rfr.rdb$relation_name
     LEFT JOIN rdb$fields fld ON rfr.rdb$field_source = fld.rdb$field_name
WHERE 
    rel.rdb$view_source IS NOT NULL AND 
  (@Owner IS NULL OR @Owner = rel.rdb$owner_name) AND
  (@TABLE_NAME IS NULL OR @TABLE_NAME = rel.rdb$relation_name)
ORDER BY rel.rdb$relation_name, rfr.rdb$field_position
";
        }

        public IList<DatabaseColumn> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@Owner", Owner);
            AddDbParameter(command, "@TABLE_NAME", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var col = new DatabaseColumn
            {
                SchemaOwner = record.GetString("OWNER_NAME").Trim(),
                TableName = record.GetString("VIEW_NAME").Trim(),
                Name = record.GetString("COLUMN_NAME").Trim(),
                Ordinal = record.GetInt("ORDINAL_POSITION"),
                Nullable = record.GetBoolean("IS_NULLABLE"),
                DefaultValue = record.GetString("COLUMN_DEFAULT"),
                DbDataType = record.GetString("FIELD_TYPE"),
                Length = record.GetNullableInt("CHARACTER_MAX_LENGTH"),
                Precision = record.GetNullableInt("NUMERIC_PRECISION"),
                Scale = record.GetNullableInt("NUMERIC_SCALE"),
            };
            Result.Add(col);
        }
    }
}
