using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    class ColumnDescriptions : SqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;

        public ColumnDescriptions(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT
    SchemaOwner = s.name,
    TableName = o.name,
    ColumnName = c.name,
    ColumnDescription = p.value
FROM sysobjects o
INNER JOIN syscolumns c
    ON o.id = c.id
INNER JOIN  sys.schemas s
    ON s.schema_id = o.uid
INNER JOIN sys.extended_properties p
    ON p.major_id = c.id
    AND	p.minor_id = c.colid
    AND	p.name = 'MS_Description'
WHERE
    (o.name = @tableName OR @tableName IS NULL) AND
    (s.name = @schemaOwner OR @schemaOwner IS NULL) AND
o.type= 'U'
ORDER BY s.name, o.name";
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