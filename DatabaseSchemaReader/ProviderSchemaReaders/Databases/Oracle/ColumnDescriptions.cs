using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class ColumnDescriptions : OracleSqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;

        public ColumnDescriptions(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT 
    OWNER AS SchemaOwner, 
    TABLE_NAME AS TableName,
    COLUMN_NAME AS ColumnName,
    COMMENTS AS ColumnDescription
FROM ALL_COL_COMMENTS
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
            var table = Result.FirstOrDefault(x => x.SchemaOwner == owner &&
                                                   x.Name == name);
            if (table == null)
            {
                table = new DatabaseTable
                        {
                            SchemaOwner = owner,
                            Name = name,
                        };
                Result.Add(table);
            }

            var col = new DatabaseColumn
                      {
                          Name = record.GetString("ColumnName"),
                          Description = record.GetString("ColumnDescription")
                      };
            table.Columns.Add(col);
        }
    }
}