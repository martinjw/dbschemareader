using DatabaseSchemaReader.ProviderSchemaReaders.Databases;
using SqlServerSchemaReader.Schema;
using System.Data;
using System.Data.Common;

namespace SqlServerSchemaReader.SchemaReaders
{
    /// <summary>
    /// Read user defined data types (alias types)
    /// </summary>
    internal class AliasTypeReader : SqlExecuter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AliasTypeReader"/> class.
        /// </summary>
        public AliasTypeReader()
        {
            Sql = @"SELECT
SCHEMA_NAME(t.schema_id) AS schema_name,
t.name,
t.max_length,
t.precision,
t.scale,
t.is_nullable,
t2.name AS system_name
FROM sys.types t
LEFT OUTER JOIN sys.types t2
    ON t.system_type_id = t2.user_type_id
WHERE t.is_user_defined = 1
AND t.is_table_type = 0
AND t.is_assembly_type = 0
AND (SCHEMA_NAME(t.schema_id) = @schema OR @schema IS NULL)
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
            var tableName = record["name"].ToString();
            var systemName = record.GetString("system_name");

            _schema.AliasTypes.Add(new AliasType
            {
                Name = tableName,
                SchemaOwner = schema,
                SystemType = systemName,
                MaxLength = record.GetNullableInt("max_length"),
                Scale = record.GetNullableInt("scale"),
                Precision = record.GetNullableInt("precision"),
                Nullable = record.GetBoolean("is_nullable"),
            });
        }
    }
}