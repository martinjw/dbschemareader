using System;
using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    internal class Indexes : SqlExecuter<DatabaseIndex>
    {
        private readonly string _tableName;

        public Indexes(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT 
    n.nspname as table_schema,
    t.relname as table_name,
    i.relname as index_name,
    a.attname as column_name
FROM
    pg_catalog.pg_class i 
JOIN
    pg_catalog.pg_index ix ON ix.indexrelid = i.oid 
JOIN
    pg_catalog.pg_class t ON ix.indrelid = t.oid 
JOIN
    pg_attribute a on t.oid = a.attrelid 
LEFT JOIN
    pg_catalog.pg_namespace n ON n.oid = i.relnamespace
WHERE
    i.relkind = 'i'
    AND n.nspname not in ('pg_catalog', 'pg_toast')
    AND pg_catalog.pg_table_is_visible(i.oid)
    AND a.attnum = ANY(ix.indkey)
    AND t.relkind = 'r'
    AND (n.nspname = :OWNER OR :OWNER IS NULL)
    AND (t.relname = :TABLENAME OR :TABLENAME IS NULL)
ORDER BY
    n.nspname, t.relname, i.relname";
        }

        public IList<DatabaseIndex> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "OWNER", Owner);
            AddDbParameter(command, "TABLENAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["table_schema"].ToString();
            var tableName = record["table_name"].ToString();
            var name = record["index_name"].ToString();
            var index = Result.FirstOrDefault(f => f.Name == name && f.SchemaOwner == schema && f.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (index == null)
            {
                index = new DatabaseIndex
                {
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                };
                Result.Add(index);
            }
            var colName = record.GetString("column_name");
            if (string.IsNullOrEmpty(colName)) return;

            var col = new DatabaseColumn
            {
                Name = colName,
            };
            index.Columns.Add(col);
        }
    }
}