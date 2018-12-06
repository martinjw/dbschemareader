using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class ComputedColumns : OracleSqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public ComputedColumns(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT 
OWNER,
TABLE_NAME AS TABLENAME, 
COLUMN_NAME AS COLUMNNAME, 
DATA_DEFAULT AS COMPUTEDDEFINITION 
FROM all_tab_cols
WHERE 
VIRTUAL_COLUMN = 'YES' AND
(TABLE_NAME = :tableName OR :tableName IS NULL) AND 
(OWNER = :schemaOwner OR :schemaOwner IS NULL) 
ORDER BY TABLE_NAME, COLUMN_NAME";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            if (Version(connectionAdapter.DbConnection) < 11)
            {
                //only supported in 11g+
                return new List<DatabaseColumn>();
            }
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "TableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("OWNER");
            var tableName = record.GetString("TableName");
            var columnName = record.GetString("ColumnName");
            var computed = record.GetString("ComputedDefinition");
            var table = new DatabaseColumn
            {
                SchemaOwner = schema,
                TableName = tableName,
                Name = columnName,
                ComputedDefinition = computed,
            };

            Result.Add(table);
        }
    }
}
