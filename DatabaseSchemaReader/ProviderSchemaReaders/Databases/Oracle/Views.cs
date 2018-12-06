using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class Views : OracleSqlExecuter<DatabaseView>
    {
        private readonly string _viewName;

        public Views(int? commandTimeout, string owner, string viewName) : base(commandTimeout, owner)
        {
            _viewName = viewName;
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

        public IList<DatabaseView> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
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
