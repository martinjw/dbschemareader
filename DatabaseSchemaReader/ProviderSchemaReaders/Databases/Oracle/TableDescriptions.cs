using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class TableDescriptions : OracleSqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;

        public TableDescriptions(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Sql = @"SELECT 
    OWNER AS SchemaOwner, 
    TABLE_NAME AS TableName,
    COMMENTS AS TableDescription
FROM ALL_TAB_COMMENTS
WHERE
    (TABLE_NAME = :tableName OR :tableName IS NULL) AND 
    (OWNER = :schemaOwner OR :schemaOwner IS NULL) AND 
    OWNER != 'SYS' AND
    COMMENTS IS NOT NULL";
        }

        public IList<DatabaseTable> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "tableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("SchemaOwner");
            var name = record.GetString("TableName");
            var table = new DatabaseTable
            {
                SchemaOwner = owner,
                Name = name,
                Description = record.GetString("TableDescription"),
            };
            Result.Add(table);
        }
    }
}