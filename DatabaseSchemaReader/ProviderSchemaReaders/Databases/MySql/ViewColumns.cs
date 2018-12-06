using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    internal class ViewColumns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _viewName;

        public ViewColumns(int? commandTimeout, string owner, string viewName)
            : base(commandTimeout, owner)
        {
            _viewName = viewName;
            Owner = owner;
            Sql = @"select c.TABLE_SCHEMA, 
c.TABLE_NAME, 
COLUMN_NAME, 
ORDINAL_POSITION, 
COLUMN_DEFAULT, 
IS_NULLABLE, 
COLUMN_TYPE, 
CHARACTER_MAXIMUM_LENGTH, 
NUMERIC_PRECISION, 
NUMERIC_SCALE
from INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.VIEWS v 
 ON c.TABLE_SCHEMA = v.TABLE_SCHEMA AND 
    c.TABLE_NAME = v.TABLE_NAME
where 
    (c.TABLE_SCHEMA = @Owner or (@Owner is null)) and 
    (c.TABLE_NAME = @TableName or (@TableName is null))
 order by 
    c.TABLE_SCHEMA, c.TABLE_NAME, ORDINAL_POSITION";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@Owner", Owner);
            AddDbParameter(command, "@TableName", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var col = new DatabaseColumn
            {
                SchemaOwner = record.GetString("TABLE_SCHEMA"),
                TableName = record.GetString("TABLE_NAME"),
                Name = record.GetString("COLUMN_NAME"),
                Ordinal = record.GetInt("ORDINAL_POSITION"),
                Nullable = record.GetBoolean("IS_NULLABLE"),
                DefaultValue = record.GetString("COLUMN_DEFAULT"),
                DbDataType = record.GetString("COLUMN_TYPE"),
                Length = record.GetNullableInt("CHARACTER_MAXIMUM_LENGTH"),
                Precision = record.GetNullableInt("NUMERIC_PRECISION"),
                Scale = record.GetNullableInt("NUMERIC_SCALE"),
                //DateTimePrecision = record.GetNullableInt("DATETIME_PRECISION"),
            };
            Result.Add(col);
        }
    }
}
