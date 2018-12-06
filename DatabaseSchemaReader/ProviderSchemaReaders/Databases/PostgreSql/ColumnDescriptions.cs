using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    class ColumnDescriptions : SqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;

        public ColumnDescriptions(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT 
    ns.nspname AS SchemaOwner, 
    c.relname AS TableName, 
    cols.column_name AS ColumnName,
    d.description AS ColumnDescription
FROM information_schema.columns cols
INNER JOIN pg_class c 
    ON c.relname=cols.table_name
INNER JOIN pg_namespace ns 
    ON c.relnamespace = ns.oid
INNER JOIN pg_description d 
    ON c.oid = d.objoid
    AND d.objsubid = cols.ordinal_position
WHERE 
    (cols.table_name = :tableName OR :tableName IS NULL) AND 
    (ns.nspname = :schemaOwner OR :schemaOwner IS NULL)";
        }

        public IList<DatabaseTable> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
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