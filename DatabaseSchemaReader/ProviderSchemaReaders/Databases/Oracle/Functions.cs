using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class Functions : OracleSqlExecuter<DatabaseFunction>
    {
        public Functions(string owner)
        {
            Owner = owner;
            Sql = @"SELECT OWNER,
  OBJECT_NAME,
  DBMS_METADATA.GET_DDL('FUNCTION', OBJECT_NAME, :OWNER) AS ""SQL""
FROM ALL_OBJECTS
WHERE (OWNER     = :OWNER
OR :OWNER       IS NULL)
AND OBJECT_TYPE  = 'FUNCTION'
ORDER BY OWNER,OBJECT_NAME";

        }

        public IList<DatabaseFunction> Execute(DbConnection connection, DbTransaction transaction)
        {
            try
            {
                ExecuteDbReader(connection, transaction);
            }
            catch (DbException ex)
            {
                System.Diagnostics.Trace.WriteLine("Error reading oracle functions " + ex.Message);
            }
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "Owner", Owner);
        }

        protected override void Mapper(IDataRecord record)
        {
            var owner = record.GetString("OWNER");
            var name = record.GetString("OBJECT_NAME");
			var sql = record.GetString("SQL");
            var sproc = new DatabaseFunction
            {
                SchemaOwner = owner,
                Name = name,
                Sql = sql,
            };
            Result.Add(sproc);
        }
    }
}
