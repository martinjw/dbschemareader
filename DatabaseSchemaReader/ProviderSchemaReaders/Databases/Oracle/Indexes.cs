using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class Indexes : OracleSqlExecuter<DatabaseIndex>
    {
        private readonly string _tableName;

        public Indexes(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Sql = @"SELECT
  cols.INDEX_OWNER,
  cols.INDEX_NAME,
  cols.TABLE_OWNER,
  cols.TABLE_NAME,
  COLUMN_NAME,
  COLUMN_POSITION,
  DESCEND, --normally ASC
  DECODE(UNIQUENESS,'UNIQUE',1,0) IsUnique,
  ix.INDEX_TYPE
FROM ALL_IND_COLUMNS cols
INNER JOIN ALL_INDEXES ix
	ON ix.OWNER = cols.INDEX_OWNER AND ix.INDEX_NAME = cols.INDEX_NAME
WHERE 
(cols.TABLE_OWNER = :TABLEOWNER OR :TABLEOWNER IS NULL)
AND (cols.TABLE_NAME = :TABLENAME OR :TABLENAME IS NULL)
AND cols.INDEX_OWNER NOT IN ('SYS', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS', 'SYSTEM')
ORDER BY cols.TABLE_OWNER,
  cols.TABLE_NAME,
  COLUMN_POSITION";

        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "TABLEOWNER", Owner);
            AddDbParameter(command, "TABLENAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("TABLE_OWNER");
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
                    IsUnique = record.GetBoolean("IsUnique"),
                    IndexType = record.GetString("INDEX_TYPE")
                };
                Result.Add(index);
            }
            var colName = record.GetString("COLUMN_NAME");
            if (string.IsNullOrEmpty(colName)) return;

            var col = new DatabaseColumn
            {
                Name = colName,
                Ordinal= record.GetNullableInt("COLUMN_POSITION").GetValueOrDefault(),
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
