using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class Views : OracleSqlExecuter<DatabaseView>
    {
        private readonly string _viewName;

        public Views(string owner, string viewName)
        {
            _viewName = viewName;
            Owner = owner;
            Sql = @"SELECT
  OWNER,
  VIEW_NAME,
  TEXT
FROM ALL_VIEWS
WHERE (OWNER = :OWNER OR :OWNER IS NULL)
AND (VIEW_NAME = :VIEWNAME OR :VIEWNAME IS NULL)
AND OWNER NOT IN ('SYS', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS', 'SYSTEM')
";
        }

        public IList<DatabaseView> Execute(DbConnection connection, DbTransaction transaction)
        {
            ExecuteDbReader(connection, transaction);
            return Result;
        }



        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "OWNER", Owner);
            AddDbParameter(command, "VIEWNAME", _viewName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record["OWNER"].ToString();
            var name = record["VIEW_NAME"].ToString();
            var table = new DatabaseView
                        {
                            Name = name,
                            SchemaOwner = schema,
                            Sql = record.GetString("TEXT"),
                        };

            Result.Add(table);
        }
    }
}
