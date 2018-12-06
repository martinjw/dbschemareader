using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    class IdentityColumns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public IdentityColumns(int? commandTimeout, string owner, string tableName)
            : base(commandTimeout, owner)
        {
            _tableName = tableName;
			//increment is always 1
            Sql = @"SELECT 
TABLE_SCHEMA AS 'SchemaOwner', 
TABLE_NAME AS 'TableName', 
COLUMN_NAME AS 'ColumnName'
FROM information_schema.columns
WHERE EXTRA = 'auto_increment' AND
(TABLE_NAME = @tableName OR @tableName IS NULL) AND 
(TABLE_SCHEMA = @schemaOwner OR @schemaOwner IS NULL)";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "@schemaOwner", Owner);
            AddDbParameter(command, "@TableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("SchemaOwner");
            var tableName = record.GetString("TableName");
            var columnName = record.GetString("ColumnName");
            var column = new DatabaseColumn
            {
                SchemaOwner = schema,
                TableName = tableName,
                Name = columnName,
				IsAutoNumber = true,
                IdentityDefinition = new DatabaseColumnIdentity(),
            };

            Result.Add(column);
        }
    }
}
