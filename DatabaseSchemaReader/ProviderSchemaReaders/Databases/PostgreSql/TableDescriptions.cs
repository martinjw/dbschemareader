using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    class TableDescriptions : SqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;

        public TableDescriptions(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT 
    ns.nspname AS SchemaOwner, 
    c.relname AS TableName, 
    d.description AS TableDescription
FROM pg_class c
INNER JOIN pg_namespace ns ON c.relnamespace = ns.oid
INNER JOIN pg_description d ON c.oid = d.objoid
WHERE 
    c.relkind = 'r' AND    
    d.objsubid = 0 AND
    (c.relname = :tableName OR :tableName IS NULL) AND 
    (ns.nspname = :schemaOwner OR :schemaOwner IS NULL)";
        }

        public IList<DatabaseTable> Execute(DbConnection connection, DbTransaction transaction)
        {
            ExecuteDbReader(connection, transaction);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
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