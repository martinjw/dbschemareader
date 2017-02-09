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
    /// Read dependent columns for user defined data types (UDTs)
    /// </summary>
    internal class AliasTypeColumnReader : SqlExecuter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AliasTypeColumnReader"/> class.
        /// </summary>
        public AliasTypeColumnReader()
        {
            Sql = @"SELECT
 SCHEMA_NAME(t.schema_id) AS schema_name,
 OBJECT_NAME(object_id) AS table_name,
 c.name AS column_name,
 TYPE_NAME(c.user_type_id) AS type_name
FROM sys.columns AS c
INNER JOIN sys.types AS t ON c.user_type_id = t.user_type_id
where t.is_user_defined = 1
AND (SCHEMA_NAME(t.schema_id) = @schema OR @schema IS NULL)";
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
            var typeName = record["type_name"].ToString();
            var at =
                _schema.AliasTypes.FirstOrDefault(
                    t => string.Equals(t.Name, typeName, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(t.SchemaOwner, schema, StringComparison.OrdinalIgnoreCase));
            if (at == null)
            {
                return;
            }

            var col = new DatabaseColumn
            {
                SchemaOwner = schema,
                TableName = record.GetString("table_name"),
                Name = record.GetString("column_name"),
                DbDataType = typeName,
            };
            at.DependentColumns.Add(col);
        }
    }
}