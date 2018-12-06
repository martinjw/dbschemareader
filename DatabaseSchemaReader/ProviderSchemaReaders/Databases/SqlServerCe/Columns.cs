using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServerCe
{
    internal class Columns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public Columns(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"select TABLE_SCHEMA, 
TABLE_NAME, 
COLUMN_NAME, 
ORDINAL_POSITION, 
COLUMN_DEFAULT, 
IS_NULLABLE, 
DATA_TYPE, 
CHARACTER_MAXIMUM_LENGTH, 
NUMERIC_PRECISION, 
NUMERIC_SCALE, 
DATETIME_PRECISION,
DESCRIPTION,
AUTOINC_SEED,
AUTOINC_INCREMENT
from INFORMATION_SCHEMA.COLUMNS
where 
    (TABLE_SCHEMA = @Owner or (@Owner is null)) and 
    (TABLE_NAME = @TableName or (@TableName is null))
 order by 
    TABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "Owner", Owner, DbType.String);
            AddDbParameter(command, "TableName", _tableName, DbType.String);
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
                DbDataType = record.GetString("DATA_TYPE"),
                Length = record.GetNullableInt("CHARACTER_MAXIMUM_LENGTH"),
                Precision = record.GetNullableInt("NUMERIC_PRECISION"),
                Scale = record.GetNullableInt("NUMERIC_SCALE"),
                DateTimePrecision = record.GetNullableInt("DATETIME_PRECISION"),
                Description = record.GetString("DESCRIPTION"),
            };
            var seed = record.GetNullableInt("AUTOINC_SEED");
            if (seed.HasValue)
            {
                var increment = record.GetNullableLong("AUTOINC_INCREMENT");
                col.IsAutoNumber = true;
                col.IdentityDefinition = new DatabaseColumnIdentity { IdentitySeed = seed.Value, IdentityIncrement = increment.Value };
            }

            Result.Add(col);
        }
    }
}
