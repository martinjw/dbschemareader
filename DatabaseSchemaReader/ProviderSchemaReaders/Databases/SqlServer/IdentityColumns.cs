using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    class IdentityColumns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public IdentityColumns(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT 
SchemaOwner = s.name, 
TableName = o.name, 
ColumnName = c.name,
IdentitySeed = seed_value,
IdentityIncrement = increment_value
FROM sys.identity_columns c
INNER JOIN sys.all_objects o ON c.object_id = o.object_id
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE 
(o.name = @TableName OR @TableName IS NULL) AND 
(s.name = @schemaOwner OR @schemaOwner IS NULL) AND 
o.type= 'U' 
ORDER BY o.name, c.name";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "TableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("SchemaOwner");
            var tableName = record.GetString("TableName");
            var columnName = record.GetString("ColumnName");
            var seed = record.GetNullableLong("IdentitySeed").GetValueOrDefault();
            var increment = record.GetNullableLong("IdentityIncrement").GetValueOrDefault();
            var column = new DatabaseColumn
            {
                SchemaOwner = schema,
                TableName = tableName,
                Name = columnName,
                IsAutoNumber = true,
                IdentityDefinition = new DatabaseColumnIdentity { IdentityIncrement = increment, IdentitySeed = seed},
            };

            Result.Add(column);
        }
    }
}
