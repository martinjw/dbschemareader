using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Converters.KeyMaps;
using DatabaseSchemaReader.ProviderSchemaReaders.Converters.RowConverters;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class ViewColumns : OracleSqlExecuter<DatabaseColumn>
    {
        private readonly string _viewName;
        private readonly ColumnRowConverter _converter;

        public ViewColumns(string owner, string viewName)
        {
            _viewName = viewName;
            Owner = owner;
            Sql = @"select c.TABLE_SCHEMA, 
c.TABLE_NAME, 
COLUMN_NAME, 
ORDINAL_POSITION, 
COLUMN_DEFAULT, 
IS_NULLABLE, 
DATA_TYPE, 
CHARACTER_MAXIMUM_LENGTH, 
NUMERIC_PRECISION, 
NUMERIC_SCALE, 
DATETIME_PRECISION 
from INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.VIEWS v 
 ON c.TABLE_SCHEMA = v.TABLE_SCHEMA AND 
    c.TABLE_NAME = v.TABLE_NAME
where 
    (c.TABLE_SCHEMA = @Owner or (@Owner is null)) and 
    (c.TABLE_NAME = @TableName or (@TableName is null))
 order by 
    c.TABLE_SCHEMA, c.TABLE_NAME, ORDINAL_POSITION";

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
            AddDbParameter(command, "TableName", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var col = _converter.Convert(record);
            Result.Add(col);
        }
    }
}
