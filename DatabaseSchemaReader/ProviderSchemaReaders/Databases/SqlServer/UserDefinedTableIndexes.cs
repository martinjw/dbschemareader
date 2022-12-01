using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    internal class UserDefinedTableIndexes : SqlExecuter<DatabaseIndex>
    {
        public UserDefinedTableIndexes(int? commandTimeout, string owner) : base(commandTimeout, owner)
        {
            Sql = @"SELECT 
    SCHEMA_NAME(tt.schema_id) AS Schema_name,
    tt.name AS table_Name,
    i.name AS index_name ,
    COL_NAME(ic.object_id, ic.column_id) AS COLUMN_NAME ,
    ic.index_column_id ,
    ic.key_ordinal AS Ordinal,
    i.is_unique AS IsUnique,
    i.is_primary_key AS IsPrimary,
    i.type_desc AS Index_type
FROM sys.indexes AS i
INNER JOIN sys.index_columns AS ic 
    ON i.object_id = ic.object_id
AND i.index_id = ic.index_id
INNER JOIN sys.table_types AS tt 
    ON i.object_id = tt.type_table_object_id
WHERE
    (SCHEMA_NAME(tt.schema_id) = @schema OR @schema IS NULL)
ORDER BY tt.name, i.name
";
        }

        /// <summary>
        /// Use this for schema level (i.e. all tables)
        /// </summary>
        public IList<DatabaseIndex> Execute(IConnectionAdapter connectionAdapter)
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
            var schema = record["schema_name"].ToString();
            var tableName = record["table_Name"].ToString();
            var indexName = record["index_name"].ToString();
            var index = Result.FirstOrDefault(f => f.Name == indexName && f.SchemaOwner == schema && f.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (index == null)
            {
                index = new DatabaseIndex
                {
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = indexName,
                    IndexType = record.GetString("Index_type"),
                    IsUnique = record.GetBoolean("IsUnique"),
                };
                if (record.GetBoolean("IsPrimary"))
                {
                    //by default SqlServer pks have clustered indexes. If they are not, we need to record it.
                    index.IndexType = string.Equals("NONCLUSTERED", index.IndexType, StringComparison.OrdinalIgnoreCase) ?
                        "PRIMARY NONCLUSTERED" :
                        "PRIMARY";
                }
                Result.Add(index);
            }

            var colName = record.GetString("COLUMN_NAME");

            var col = new DatabaseColumn
            {
                Name = colName,
                Ordinal = record.GetInt("Ordinal"),
            };
            index.Columns.Add(col);
        }
    }
}