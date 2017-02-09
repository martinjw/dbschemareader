using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases;
using SqlServerSchemaReader.Schema;
using System;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace SqlServerSchemaReader.SchemaReaders
{
    /// <summary>
    /// Read user defined table types (UDTs)
    /// </summary>
    internal class TableTypeReader : SqlExecuter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TableTypeReader"/> class.
        /// </summary>
        public TableTypeReader()
        {
            Sql = @"SELECT
SCHEMA_NAME(tt.schema_id) AS schema_name,
tt.name AS TYPE_NAME,
c.name AS COLUMN_NAME,
st.name AS DATA_TYPE,
c.max_length,
c.precision,
c.scale,
c.is_nullable,
c.column_id,
object_definition(c.default_object_id) AS default_value,
cc.definition AS computed_definition
FROM sys.table_types tt
INNER JOIN sys.columns c
   ON c.object_id = tt.type_table_object_id
INNER JOIN sys.types st
   ON st.system_type_id = c.system_type_id
LEFT OUTER JOIN sys.computed_columns cc
   ON cc.object_id = c.object_id AND c.name = cc.name
WHERE
    st.name <> 'sysname'
AND (SCHEMA_NAME(tt.schema_id) = @schema OR @schema IS NULL)
ORDER BY tt.name, c.column_id
";
        }

        private SqlServerSchema _schema;

        /// <summary>
        /// Use this for schema level (i.e. all tables)
        /// </summary>
        public void Execute(SqlServerSchema schema, DbConnection connection)
        {
            _schema = schema;
            ExecuteDbReader(connection);
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
            var schema = record["schema_name"].ToString();
            var typeName = record["TYPE_NAME"].ToString();
            var tt =
                _schema.TableTypes.FirstOrDefault(
                    t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(t.SchemaOwner, schema, StringComparison.OrdinalIgnoreCase));
            if (tt == null)
            {
                tt = new TableType
                {
                    SchemaOwner = schema,
                    Name = typeName
                };
                _schema.TableTypes.Add(tt);
            }

            var colName = record.GetString("COLUMN_NAME");

            var col = new DatabaseColumn
            {
                Name = colName,
                SchemaOwner = schema,
                TableName = typeName,
                DbDataType = record.GetString("DATA_TYPE"),
                Nullable = record.GetBoolean("is_nullable"),
                Length = record.GetNullableInt("max_length"),
                Precision = record.GetNullableInt("precision"),
                Scale = record.GetNullableInt("scale"),
                Ordinal = record.GetInt("column_id"),
                DefaultValue = record.GetString("default_value"),
                ComputedDefinition = record.GetString("computed_definition"),
            };
            tt.Columns.Add(col);
        }
    }
}