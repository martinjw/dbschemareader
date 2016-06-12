using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class ProcedureArguments : OracleSqlExecuter<DatabaseArgument>
    {
        private readonly string _name;

        public ProcedureArguments(string owner, string name)
        {
            _name = name;
            Owner = owner;
            Sql = @"SELECT
  OWNER,
  PACKAGE_NAME,
  OBJECT_NAME,
  ARGUMENT_NAME,
  POSITION,
  SEQUENCE,
  DEFAULT_VALUE,
  DEFAULT_LENGTH,
  IN_OUT,
  DATA_LENGTH,
  DATA_PRECISION,
  DATA_SCALE,
  DATA_TYPE
FROM ALL_ARGUMENTS
WHERE (OWNER = :OWNER OR :OWNER IS NULL)
AND (OBJECT_NAME = :PROCEDURENAME OR :PROCEDURENAME IS NULL)
ORDER BY ";
        }

        public IList<DatabaseArgument> Execute(DbConnection connection)
        {
            ExecuteDbReader(connection);
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "OWNER", Owner);
            AddDbParameter(command, "PROCEDURENAME", _name);
        }

        protected override void Mapper(IDataRecord record)
        {
            //var col = _converter.Convert(record);
            //Result.Add(col);
        }
    }
}