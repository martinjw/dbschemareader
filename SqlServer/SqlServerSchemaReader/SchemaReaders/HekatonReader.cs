using System;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases;
using SqlServerSchemaReader.Schema;

namespace SqlServerSchemaReader.SchemaReaders
{
    /// <summary>
    /// Read if table(s) are memory optimized (SqlServer 2014 and 2016 only)
    /// </summary>
    /// <seealso cref="DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlExecuter" />
    public class HekatonReader : SqlExecuter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HekatonReader"/> class.
        /// </summary>
        public HekatonReader()
        {
            Sql = @"SELECT
SCHEMA_NAME(schema_id) AS schema_name,
name AS table_name
FROM sys.tables
WHERE is_memory_optimized = 1
AND (SCHEMA_NAME(schema_id) = @schema OR @schema IS NULL)
AND (name = @tableName OR @tableName IS NULL)";
        }

        private SqlServerTable _table;
        private SqlServerSchema _schema;

        /// <summary>
        /// Use this for schema level (i.e. all tables)
        /// </summary>
        public void Execute(SqlServerSchema schema, IConnectionAdapter connection)
        {
            if (!HasHekaton(connection)) return;

            _schema = schema;
            ExecuteDbReader(connection);
        }

        private static bool HasHekaton(IConnectionAdapter connectionAdapter)
        {
            var connection = connectionAdapter.DbConnection;
            var cmd = connection.CreateCommand();
            //step 1- check if there are any sequences (backwards compatible)
            cmd.CommandText = @"select COL_LENGTH('sys.tables','is_memory_optimized')";
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            var hasHekaton = cmd.ExecuteScalar() != DBNull.Value;
            return hasHekaton;
        }

        /// <summary>
        /// Use this for a specific table
        /// </summary>
        public void Execute(SqlServerTable table, IConnectionAdapter connection)
        {
            if (!HasHekaton(connection)) return;

            _table = table;
            ExecuteDbReader(connection);
        }

        /// <summary>
        /// Add parameter(s).
        /// </summary>
        /// <param name="command">The command.</param>
        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schema", Owner);
            AddDbParameter(command, "tableName", _table?.Name);
        }

        /// <summary>
        /// Map the result ADO record to the result.
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void Mapper(IDataRecord record)
        {
            if (_table != null)
            {
                _table.IsMemoryOptimized = true;
                return;
            }
            var schema = record["schema_name"].ToString();
            var tableName = record["table_name"].ToString();
            var table = _schema.FindTableByName(tableName, schema) as SqlServerTable;
            if (table != null)
            {
                table.IsMemoryOptimized = true;
            }
        }
    }
}