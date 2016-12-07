using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    internal class Columns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public Columns(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT
     rel.rdb$owner_name AS OWNER_NAME,
     rfr.rdb$relation_name AS TABLE_NAME,
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
     CAST(fld.rdb$character_length AS integer) AS CHARACTER_MAXIMUM_LENGTH,
     CAST(fld.rdb$field_length AS integer) AS CHARACTER_OCTET_LENGTH,
     rfr.rdb$field_position AS ORDINAL_POSITION,
     rfr.rdb$default_source AS COLUMN_DEFAULT,
     fld.rdb$computed_source AS COMPUTED_SOURCE,
     coalesce(fld.rdb$null_flag, rfr.rdb$null_flag) AS IS_NULLABLE,
     rfr.rdb$description AS DESCRIPTION,
     rfr.rdb$generator_name AS SEQUENCE
FROM rdb$relation_fields rfr
     LEFT JOIN rdb$relations rel ON rfr.rdb$relation_name = rel.rdb$relation_name
     LEFT JOIN rdb$fields fld ON rfr.rdb$field_source = fld.rdb$field_name
WHERE 
  rfr.rdb$system_flag = 0 AND
  (@Owner IS NULL OR @Owner = rel.rdb$owner_name) AND
  (@TABLE_NAME IS NULL OR @TABLE_NAME = rfr.rdb$relation_name)
ORDER BY rfr.rdb$relation_name, rfr.rdb$field_position
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
            AddDbParameter(command, "@TABLE_NAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            //overflow protection
            var length = record.GetNullableLong("CHARACTER_MAXIMUM_LENGTH");
            var maxLength = (length > int.MaxValue) ? int.MaxValue : (int?)length;
            var col = new DatabaseColumn
            {
                SchemaOwner = record.GetString("OWNER_NAME").Trim(),
                TableName = record.GetString("TABLE_NAME").Trim(),
                Name = record.GetString("COLUMN_NAME").Trim(),
                Ordinal = record.GetInt("ORDINAL_POSITION"),
                Nullable = record.GetBoolean("IS_NULLABLE"),
                DefaultValue = record.GetString("COLUMN_DEFAULT"),
                DbDataType = record.GetString("DATA_TYPE"),
                Length = maxLength,
                Precision = record.GetNullableInt("NUMERIC_PRECISION"),
                Scale = record.GetNullableInt("NUMERIC_SCALE"),
                Description = record.GetString("DESCRIPTION"),
                ComputedDefinition = record.GetString("COMPUTED_SOURCE"),
                
            };
            var seq = record.GetString("SEQUENCE");
            if (!string.IsNullOrEmpty(seq))
            {
                col.IsAutoNumber = true;
                col.IdentityDefinition = new DatabaseColumnIdentity();
            }
            Result.Add(col);
        }
    }
}
