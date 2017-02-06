using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases;
using SqlServerSchemaReader.Schema;

namespace SqlServerSchemaReader.SchemaReaders
{
    /// <summary>
    /// Read the statistics
    /// </summary>
    /// <seealso cref="DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlExecuter" />
    public class StatisticsReader : SqlExecuter
    {
        private readonly string _owner;
        private readonly string _tableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StatisticsReader"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="tableName">Name of the table.</param>
        public StatisticsReader(string owner, string tableName)
        {
            _owner = owner;
            _tableName = tableName;
            Sql = @"SELECT
    SchemaName = SCHEMA_NAME(t.schema_id),
    TableName = t.name,
    StatisticsName = s.name,
    ColumnName = col.name
FROM
    sys.stats s
INNER JOIN
    sys.stats_columns stc ON s.object_id = stc.object_id and s.stats_id = stc.stats_id
INNER JOIN
    sys.columns col ON stc.object_id = col.object_id and stc.column_id = col.column_id
INNER JOIN
    sys.tables t ON s.object_id = t.object_id
WHERE
    (t.name = @TableName OR @TableName IS NULL) AND
    (SCHEMA_NAME(t.schema_id) = @schemaOwner OR @schemaOwner IS NULL) AND
    t.is_ms_shipped = 0
ORDER BY
        t.name, s.name, col.name
";
        }

        private List<DatabaseStatistics> Result { get; } = new List<DatabaseStatistics>();

        /// <summary>
        /// Executes the specified database connection.
        /// </summary>
        /// <param name="dbConnection">The database connection.</param>
        /// <returns></returns>
        public IList<DatabaseStatistics> Execute(DbConnection dbConnection)
        {
            ExecuteDbReader(dbConnection);
            return Result;
        }

        /// <summary>
        /// Add parameter(s).
        /// </summary>
        /// <param name="command">The command.</param>
        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", _owner);
            AddDbParameter(command, "TableName", _tableName);
        }

        /// <summary>
        /// Map the result ADO record to the result.
        /// </summary>
        /// <param name="record">The record.</param>
        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("SchemaName");
            var tableName = record.GetString("TableName");
            var name = record.GetString("StatisticsName");
            var statistics = Result.FirstOrDefault(f => f.Name == name && f.SchemaOwner == schema && f.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (statistics == null)
            {
                statistics = new DatabaseStatistics
                {
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                };

                Result.Add(statistics);
            }
            var colName = record.GetString("ColumnName");
            if (string.IsNullOrEmpty(colName))
                return;

            statistics.Columns.Add(colName);
        }
    }
}