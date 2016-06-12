using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class StoredProcedures : OracleSqlExecuter<DatabaseStoredProcedure>
    {
        private readonly string _name;

        public StoredProcedures(string owner, string name)
        {
            _name = name;
            Owner = owner;
            Sql = @"SELECT
  OWNER,
  OBJECT_NAME
FROM ALL_OBJECTS
WHERE (OWNER = :OWNER OR :OWNER IS NULL)
AND (OBJECT_NAME = :NAME OR :NAME IS NULL)
AND OBJECT_TYPE = 'PROCEDURE'
AND OWNER NOT IN ('SYS', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS', 'SYSTEM')
";

        }

        public IList<DatabaseStoredProcedure> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "Owner", Owner);
            AddDbParameter(command, "Name", _name);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("OWNER");
            var name = record.GetString("OBJECT_NAME");
            var sproc = new DatabaseStoredProcedure
            {
                SchemaOwner = owner,
                Name = name,
            };
            Result.Add(sproc);
        }
    }
}
