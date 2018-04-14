﻿using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

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
  OBJECT_NAME,
  PROCEDURE_NAME,
  OBJECT_TYPE
FROM ALL_PROCEDURES
WHERE (OWNER = :OWNER OR :OWNER IS NULL)
AND (OBJECT_NAME = :NAME OR :NAME IS NULL)
AND (OBJECT_TYPE = 'PROCEDURE' OR OBJECT_TYPE = 'PACKAGE')
AND NOT (PROCEDURE_NAME IS NULL AND OBJECT_TYPE = 'PACKAGE')
AND OWNER NOT IN ('SYS', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS', 'SYSTEM')
";

        }

        public IList<DatabaseStoredProcedure> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
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
            string pack = null;
            var owner = record.GetString("OWNER");
            var name = record.GetString("OBJECT_NAME");
            var procName = record.GetString("PROCEDURE_NAME");
            if (procName != null)
            {
                pack = name;
                name = procName;
            }
            var sproc = new DatabaseStoredProcedure
            {
                SchemaOwner = owner,
                Package = pack,
                Name = name,
            };
            Result.Add(sproc);
        }
    }
}
