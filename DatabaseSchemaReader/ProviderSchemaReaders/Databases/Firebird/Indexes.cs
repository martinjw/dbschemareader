using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    class Indexes : SqlExecuter<DatabaseIndex>
    {
        private readonly string _tableName;

        public Indexes(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @" SELECT 
rel.rdb$owner_name as owner,
i.rdb$relation_name as table_name,
i.rdb$index_name as index_name,
isg.rdb$field_name as column_name,
(isg.rdb$field_position + 1) as field_position,
i.rdb$unique_flag as is_unique,
rc.rdb$constraint_type as constraint_type
FROM rdb$index_segments isg
LEFT JOIN rdb$indices i on i.rdb$index_name = isg.rdb$index_name
LEFT JOIN rdb$relation_constraints rc on rc.rdb$index_name = isg.rdb$index_name
LEFT JOIN rdb$relations rel on i.rdb$relation_name = rel.rdb$relation_name
WHERE 
  (@owner IS NULL OR @owner = rel.rdb$owner_name) AND
  (@tableName IS NULL OR @tableName = rel.rdb$relation_name)
 ORDER BY i.rdb$relation_name, i.rdb$index_name, isg.rdb$field_position";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@owner", Owner);
            AddDbParameter(command, "@tableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("owner").TrimEnd();
            var tableName = record.GetString("table_name").TrimEnd();
            var name = record.GetString("index_name").TrimEnd();
            var index = Result.FirstOrDefault(f => f.Name == name && f.SchemaOwner == schema && f.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (index == null)
            {
                index = new DatabaseIndex
                {
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                    IndexType = record.GetString("constraint_type"),
                    IsUnique = record.GetBoolean("is_unique"),
                };
                Result.Add(index);
            }
            var colName = record.GetString("column_name").TrimEnd();
            if (string.IsNullOrEmpty(colName)) return;

            var col = new DatabaseColumn
            {
                Name = colName,
                Ordinal = record.GetInt("field_position"),
            };
            index.Columns.Add(col);
        }

        public IList<DatabaseIndex> Execute(DbConnection dbConnection)
        {
            ExecuteDbReader(dbConnection);
            return Result;
        }
    }
}
