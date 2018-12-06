using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class Tables : OracleSqlExecuter<DatabaseTable>
    {
        private readonly string _tableName;

        public Tables(int? commandTimeout, string owner, string tableName) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            Sql = @"SELECT
  OWNER,
  TABLE_NAME
FROM ALL_TABLES
WHERE 
    (OWNER=:OWNER or :OWNER IS NULL) AND 
    (TABLE_NAME = :TABLENAME or :TABLENAME IS NULL) AND
    OWNER NOT IN ('SYS', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS', 'SYSTEM')
ORDER BY OWNER, TABLE_NAME";
        }

        public IList<DatabaseTable> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }



        protected override void AddParameters(DbCommand command)
        {
            base.AddParameters(command);
            AddDbParameter(command, "OWNER", Owner);
            AddDbParameter(command, "TABLENAME", _tableName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["OWNER"].ToString();
            var name = record["TABLE_NAME"].ToString();
            var table = new DatabaseTable
                        {
                            Name = name,
                            SchemaOwner = schema
                        };

            Result.Add(table);
        }
    }
}
