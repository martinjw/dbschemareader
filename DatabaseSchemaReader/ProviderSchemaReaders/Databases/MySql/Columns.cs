using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    internal class Columns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public Columns(int? commandTimeout, string owner, string tableName)
            : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Sql = @"select c.TABLE_SCHEMA, 
c.TABLE_NAME, 
COLUMN_NAME, 
ORDINAL_POSITION, 
COLUMN_DEFAULT, 
IS_NULLABLE, 
DATA_TYPE, 
COLUMN_TYPE,
CHARACTER_MAXIMUM_LENGTH, 
NUMERIC_PRECISION, 
NUMERIC_SCALE, 
COLUMN_COMMENT
from INFORMATION_SCHEMA.COLUMNS c
JOIN INFORMATION_SCHEMA.TABLES t 
 ON c.TABLE_SCHEMA = t.TABLE_SCHEMA AND 
    c.TABLE_NAME = t.TABLE_NAME
where 
    (c.TABLE_SCHEMA = @Owner or (@Owner is null)) and 
	(c.TABLE_SCHEMA NOT IN ('information_schema', 'mysql', 'performance_schema')) and
    (c.TABLE_NAME = @TableName or (@TableName is null)) AND
    TABLE_TYPE = 'BASE TABLE'
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
            AddDbParameter(command, "@TableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            //overflow protection
            var length = record.GetNullableLong("CHARACTER_MAXIMUM_LENGTH");
            var maxLength = (length > int.MaxValue) ? int.MaxValue : (int?)length;
            var col = new DatabaseColumn
            {
                SchemaOwner = record.GetString("TABLE_SCHEMA"),
                TableName = record.GetString("TABLE_NAME"),
                Name = record.GetString("COLUMN_NAME"),
                Ordinal = record.GetInt("ORDINAL_POSITION"),
                Nullable = record.GetBoolean("IS_NULLABLE"),
                DefaultValue = record.GetString("COLUMN_DEFAULT"),
                DbDataType = record.GetString("COLUMN_TYPE"),
                Length = maxLength,
                Precision = record.GetNullableInt("NUMERIC_PRECISION"),
                Scale = record.GetNullableInt("NUMERIC_SCALE"),
                //DateTimePrecision = record.GetNullableInt("DATETIME_PRECISION"), //added in MySQL 5.6.4. 
                Description = record.GetString("COLUMN_COMMENT"),
            };
            Result.Add(col);
        }
    }
}
