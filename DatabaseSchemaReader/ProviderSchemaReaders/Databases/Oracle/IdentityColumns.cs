using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class IdentityColumns : OracleSqlExecuter<DatabaseColumn>
    {
        private readonly string _tableName;

        public IdentityColumns(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Sql = @"SELECT
OWNER AS SchemaOwner,
TABLE_NAME AS TABLENAME,
COLUMN_NAME AS COLUMNNAME,
GENERATION_TYPE,
IDENTITY_OPTIONS
FROM all_tab_identity_cols
WHERE
(TABLE_NAME = :tableName OR :tableName IS NULL) AND
(OWNER = :schemaOwner OR :schemaOwner IS NULL)
ORDER BY TABLE_NAME, COLUMN_NAME";
        }

        public IList<DatabaseColumn> Execute(IConnectionAdapter connectionAdapter)
        {
            if (Version(connectionAdapter.DbConnection) < 12)
            {
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
            var schema = record.GetString("SchemaOwner");
            var tableName = record.GetString("TableName");
            var columnName = record.GetString("ColumnName");
            var column = new DatabaseColumn
            {
                SchemaOwner = schema,
                TableName = tableName,
                Name = columnName,
                IdentityDefinition = new DatabaseColumnIdentity(),
            };
            var options = record.GetString("IDENTITY_OPTIONS");
            ParseIdentityOptions(column.IdentityDefinition, options);
            if (string.Equals(record.GetString("GENERATION_TYPE"), "BY DEFAULT", StringComparison.OrdinalIgnoreCase))
            {
                column.IdentityDefinition.IdentityByDefault = true;
            }

            Result.Add(column);
        }

        private static void ParseIdentityOptions(DatabaseColumnIdentity identityDefinition, string options)
        {
            //START WITH: 1, INCREMENT BY: 1, MAX_VALUE: 9999999999999999999999999999, MIN_VALUE: 1, CYCLE_FLAG: N, CACHE_SIZE: 20, ORDER_FLAG: N
            //defensive in case format changes
            if (string.IsNullOrEmpty(options)) return;

            var number = ExtractBetween(options, "START WITH: ", ',');
            if (string.IsNullOrEmpty(number)) return;
            long seed;
            if (long.TryParse(number, out seed))
            {
                identityDefinition.IdentitySeed = seed;
            }

            number = ExtractBetween(options, "INCREMENT BY: ", ',');
            if (string.IsNullOrEmpty(number)) return;
            if (long.TryParse(number, out seed))
            {
                identityDefinition.IdentityIncrement = seed;
            }
        }

        private static string ExtractBetween(string haystack, string prefix, char suffix)
        {
            var start = haystack.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (start == -1) return null;
            start = start + prefix.Length;
            var end = haystack.IndexOf(suffix, start);
            return haystack.Substring(start, end - start);
        }
    }
}