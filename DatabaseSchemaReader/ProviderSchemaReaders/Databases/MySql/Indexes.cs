using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    class Indexes : SqlExecuter<DatabaseIndex>
    {
        private readonly string _tableName;

        public Indexes(int? commandTimeout, string owner, string tableName)
            : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Sql = @" SELECT 
     TABLE_SCHEMA,
     TABLE_NAME,
     INDEX_NAME,
     COLUMN_NAME,
     SEQ_IN_INDEX,
     INDEX_TYPE,
     NON_UNIQUE
FROM 
     INFORMATION_SCHEMA.STATISTICS 
WHERE 
    (TABLE_NAME = @TableName OR @TableName IS NULL) AND 
    (TABLE_SCHEMA = @schemaOwner OR @schemaOwner IS NULL)
ORDER BY 
     TABLE_SCHEMA, TABLE_NAME, INDEX_NAME, SEQ_IN_INDEX";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@schemaOwner", Owner);
            AddDbParameter(command, "@tableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("TABLE_SCHEMA");
            var tableName = record.GetString("TABLE_NAME");
            var name = record.GetString("INDEX_NAME");
            var index = Result.FirstOrDefault(f => f.Name == name && f.SchemaOwner == schema && f.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (index == null)
            {
                index = new DatabaseIndex
                {
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                    IndexType = record.GetString("INDEX_TYPE"),
                    IsUnique = !record.GetBoolean("NON_UNIQUE"),
                };
                Result.Add(index);
            }
            var colName = record.GetString("COLUMN_NAME");
            if (string.IsNullOrEmpty(colName)) return;

            var col = new DatabaseColumn
            {
                Name = colName,
                Ordinal = record.GetInt("SEQ_IN_INDEX"),
            };
            index.Columns.Add(col);
        }

        public IList<DatabaseIndex> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
