using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Converters.KeyMaps;
using DatabaseSchemaReader.ProviderSchemaReaders.Converters.RowConverters;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class Columns : OracleSqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;
        private readonly ColumnRowConverter _converter;

        public Columns(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT OWNER,
  TABLE_NAME,
  COLUMN_NAME,
  COLUMN_ID      AS ID,
  DATA_TYPE      AS DataType,
  CHAR_LENGTH    AS LENGTH,
  DATA_LENGTH    AS DATALENGTH,
  DATA_PRECISION AS PRECISION,
  DATA_SCALE     AS Scale,
  NULLABLE       AS Nullable,
  DATA_DEFAULT   AS Column_default
FROM ALL_TAB_COLUMNS
WHERE 
TABLE_NAME NOT LIKE 'BIN$%'
AND (OWNER = :OWNER OR :OWNER IS NULL)
AND OWNER NOT IN ('SYS', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS', 'SYSTEM')
AND (TABLE_NAME  = :TABLENAME OR :TABLENAME IS NULL)
ORDER BY OWNER, TABLE_NAME, ID";

            var keyMap = new ColumnsKeyMap();
            _converter = new ColumnRowConverter(keyMap);
        }

        public IList<DatabaseColumn> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "Owner", Owner);
            AddDbParameter(command, "TableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var col = _converter.Convert(record);
            Result.Add(col);
        }
    }
}
