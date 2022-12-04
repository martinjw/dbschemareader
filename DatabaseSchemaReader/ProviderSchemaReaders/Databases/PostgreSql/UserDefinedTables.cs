using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class UserDefinedTables : SqlExecuter<UserDefinedTable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataTypes"/> class.
        /// </summary>
        public UserDefinedTables(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"select 
    ns.nspname AS schemaname,
    c.relname AS tableName,
    a.attname AS columnName,
    pg_catalog.format_type(a.atttypid, a.atttypmod) as dbType,
    attnum AS ordinal,
    attnotnull As NotNullable,
    CASE atttypid
        WHEN 1042 /* char */ THEN NULLIF(a.atttypmod,-1)
        WHEN 1043 /* varchar */ THEN NULLIF(a.atttypmod,-1)-4
        END   AS maxLength
FROM 
pg_catalog.pg_class c
JOIN pg_catalog.pg_namespace ns
    ON ns.oid = c.relnamespace 
JOIN pg_catalog.pg_attribute a ON a.attrelid  = c.oid
where 
    c.relkind = 'c' --composite types
    AND ns.nspname NOT LIKE 'pg_%'
    AND ns.nspname != 'information_schema'
    AND (ns.nspname = :schema OR :schema IS NULL)
order by ns.nspname,c.relname, a.attnum
";
        }

        /// <summary>
        /// Use this for schema level (i.e. all tables)
        /// </summary>
        public IList<UserDefinedTable> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        /// <summary>
        /// Add parameter(s).
        /// </summary>
        /// <param name="command">The command.</param>
        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schema", Owner);
        }

        /// <summary>
        /// Map the result ADO record to the result.
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void Mapper(IDataRecord record)
        {
            var schema = record["schemaname"].ToString();
            var typeName = record["tableName"].ToString();
            var tt =
                Result.FirstOrDefault(
                    t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(t.SchemaOwner, schema, StringComparison.OrdinalIgnoreCase));
            if (tt == null)
            {
                tt = new UserDefinedTable
                {
                    SchemaOwner = schema,
                    Name = typeName
                };
                Result.Add(tt);
            }

            var colName = record.GetString("columnName");

            var col = new DatabaseColumn
            {
                Name = colName,
                SchemaOwner = schema,
                TableName = typeName,
                DbDataType = record.GetString("dbType"),
                Nullable = record.GetBoolean("NotNullable"),
                Length = record.GetNullableInt("maxLength"),
                Ordinal = record.GetInt("ordinal"),
            };
            tt.Columns.Add(col);

        }
    }
}