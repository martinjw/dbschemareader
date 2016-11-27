using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    class ComputedColumns : SqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public ComputedColumns(string owner, string tableName)
        {
            _tableName = tableName;
            Owner = owner;
            Sql = @"SELECT 
TABLE_SCHEMA, 
TABLE_NAME, 
COLUMN_NAME, 
GENERATION_EXPRESSION
from INFORMATION_SCHEMA.COLUMNS
WHERE 
(TABLE_NAME = @tableName OR @tableName IS NULL) AND 
(TABLE_SCHEMA = @schemaOwner OR @schemaOwner IS NULL) AND  
GENERATION_EXPRESSION  <> ''
ORDER BY TABLE_SCHEMA,TABLE_NAME";
        }

        public IList<DatabaseColumn> Execute(DbConnection connection)
        {
            var hasGeneratedColumns = false; //introduced in MySQL 5.7.6
            var cmd = connection.CreateCommand();
            //step 1- check what's in info schema (backwards compatible)
            cmd.CommandText = @"SELECT * 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE
(TABLE_SCHEMA = @schemaOwner OR @schemaOwner IS NULL)
LIMIT 1";
            AddDbParameter(cmd, "schemaOwner", Owner);
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    for (var i = 0; i < dr.FieldCount; i++)
                    {
                        if (dr.GetName(i).Equals("GENERATION_EXPRESSION", StringComparison.OrdinalIgnoreCase))
                            hasGeneratedColumns = true;
                    }
                }
            }
            if (!hasGeneratedColumns)
            {
                return new List<DatabaseColumn>();
            }

            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "tableName", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("TABLE_SCHEMA");
            var tableName = record.GetString("TABLE_NAME");
            var columnName = record.GetString("COLUMN_NAME");
            var computed = record.GetString("GENERATION_EXPRESSION");
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
