using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class ProcedureArguments : OracleSqlExecuter<DatabaseArgument>
    {
        private readonly string _name;

        public ProcedureArguments(int? commandTimeout, string owner, string name) : base(commandTimeout, owner)
        {
            _name = name;
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
ORDER BY OWNER, PACKAGE_NAME, OBJECT_NAME, POSITION";
        }

        public IList<DatabaseArgument> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
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
            var owner = record.GetString("OWNER");
            var packName = record.GetString("PACKAGE_NAME");
            var sprocName = record.GetString("OBJECT_NAME");
            var name = record.GetString("ARGUMENT_NAME");
            var arg = new DatabaseArgument
            {
                SchemaOwner = owner,
                PackageName = packName,
                ProcedureName = sprocName,
                Name = name,
                Ordinal = record.GetInt("POSITION"),
                DatabaseDataType = record.GetString("DATA_TYPE"),
                Length = record.GetNullableInt("DATA_LENGTH"),
                Precision = record.GetNullableInt("DATA_PRECISION"),
                Scale = record.GetNullableInt("DATA_SCALE"),
                
            };
            string inout = record.GetString("IN_OUT");
            if (inout.Contains("IN")) arg.In = true;
            //can be INOUT
            if (inout.Contains("OUT")) arg.Out = true;

            Result.Add(arg);
        }
    }
}