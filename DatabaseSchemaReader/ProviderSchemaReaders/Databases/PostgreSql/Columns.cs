using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Columns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public Columns(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT
  table_schema,
  table_name,
  column_name,
  ordinal_position,
  column_default,
  is_nullable,
  udt_name AS data_type,
  character_maximum_length,
  numeric_precision,
  numeric_precision_radix,
  numeric_scale,
  datetime_precision
FROM information_schema.columns
WHERE (table_schema = :OWNER OR :OWNER IS NULL)
AND (table_name = :TABLENAME OR :TABLENAME IS NULL)

UNION

-- specialized query for materialized views, because INFORMATION_SCHEMA does not support them
SELECT
    n.nspname                               AS table_schema,
    c.relname                               AS table_name,
    a.attname                               AS column_name,
    a.attnum                                AS ordinal_position,
    pg_get_expr(ad.adbin, ad.adrelid)       AS column_default,
    CASE a.attnotnull
        WHEN true  THEN 'NO'
        ELSE 'YES'
    END                                     AS is_nullable,
    t.typname                               AS data_type,
    CASE
        WHEN t.typname IN ('varchar','bpchar')
            THEN a.atttypmod - 4
        ELSE NULL
    END                                     AS character_maximum_length,
    CASE
        WHEN t.typname IN ('numeric','decimal')
            THEN ((a.atttypmod - 4) >> 16) & 65535
        ELSE NULL
    END                                     AS numeric_precision,
    CASE
        WHEN t.typname IN ('numeric','decimal')
            THEN 10
        ELSE NULL
    END                                     AS numeric_precision_radix,
    CASE
        WHEN t.typname IN ('numeric','decimal')
            THEN (a.atttypmod - 4) & 65535
        ELSE NULL
    END                                     AS numeric_scale,
    NULL::integer                           AS datetime_precision
FROM pg_class c
JOIN pg_namespace n
  ON n.oid = c.relnamespace
JOIN pg_attribute a
  ON a.attrelid = c.oid
JOIN pg_type t
  ON t.oid = a.atttypid
LEFT JOIN pg_attrdef ad
  ON ad.adrelid = c.oid
 AND ad.adnum   = a.attnum
WHERE c.relkind = 'm'                       -- materialized views
  AND a.attnum > 0
  AND NOT a.attisdropped
  AND (n.nspname = :OWNER OR :OWNER IS NULL)
  AND (c.relname = :TABLENAME OR :TABLENAME IS NULL)

ORDER BY
    table_schema,
    table_name,
    ordinal_position
";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "OWNER", Owner);
            AddDbParameter(command, "TABLENAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["table_schema"].ToString();
            var tableName = record["table_name"].ToString();
            var name = record["column_name"].ToString();
            var table = new DatabaseColumn
            {
                SchemaOwner = schema,
                TableName = tableName,
                Name = name,
                Ordinal = record.GetInt("ordinal_position"),
                DbDataType = record.GetString("data_type"),
                Length = record.GetNullableInt("character_maximum_length"),
                Precision = record.GetNullableInt("numeric_precision"),
                PrecisionRadix = record.GetNullableInt("numeric_precision_radix"),
                Scale = record.GetNullableInt("numeric_scale"),
                Nullable = record.GetBoolean("is_nullable"),
                DefaultValue = record.GetString("column_default"),
                DateTimePrecision = record.GetNullableInt("datetime_precision"),
            };

            Result.Add(table);
        }
    }
}