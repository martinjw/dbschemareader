using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class ViewColumns : OracleSqlExecuter<DatabaseColumn>
    {
        private readonly string _viewName;

        public ViewColumns(int? commandTimeout, string owner, string viewName) : base(commandTimeout, owner)
        {
            _viewName = viewName;
            Sql = @"SELECT c.OWNER,
  TABLE_NAME,
  COLUMN_NAME,
  COLUMN_ID      AS ordinal_position,
  DATA_TYPE,
  CHAR_LENGTH,
  DATA_LENGTH,
  DATA_PRECISION,
  DATA_SCALE,
  NULLABLE,
  DATA_DEFAULT
FROM ALL_TAB_COLUMNS c
INNER JOIN ALL_VIEWS v ON c.OWNER = v.OWNER AND c.TABLE_NAME = v.VIEW_NAME
WHERE
TABLE_NAME NOT LIKE 'BIN$%'
AND (c.OWNER = :OWNER OR :OWNER IS NULL)
AND c.OWNER NOT IN ('SYS', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS', 'SYSTEM')
AND (TABLE_NAME  = :TABLENAME OR :TABLENAME IS NULL)
ORDER BY c.OWNER, TABLE_NAME, COLUMN_ID";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "Owner", Owner);
            AddDbParameter(command, "TableName", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("OWNER");
            var tableName = record.GetString("TABLE_NAME");
            var name = record.GetString("COLUMN_NAME");

            var col = new DatabaseColumn
            {
                SchemaOwner = owner,
                TableName = tableName,
                Name = name,
                Ordinal = record.GetNullableInt("ordinal_position").GetValueOrDefault(),
                DbDataType = record.GetString("DATA_TYPE"),
                Length = record.GetNullableInt("CHAR_LENGTH"),
                Precision = record.GetNullableInt("DATA_PRECISION"),
                Scale = record.GetNullableInt("DATA_SCALE"),
                Nullable = record.GetBoolean("NULLABLE"),
            };
            if (col.Length < 1)
            {
                col.Length = record.GetNullableInt("DATA_LENGTH");
            }
            var d = record.GetString("DATA_DEFAULT");
            if (!string.IsNullOrEmpty(d)) d = d.Trim(' ', '\'', '=');
            col.DefaultValue = d;
            Result.Add(col);
        }
    }
}
