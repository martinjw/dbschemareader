using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class ProcedureSources : OracleSqlExecuter<ProcedureSource>
    {
        private readonly string _name;

        public ProcedureSources(int? commandTimeout, string owner, string name) : base(commandTimeout, owner)
        {
            _name = name;
            Sql = @"SELECT 
OWNER, 
NAME, 
TYPE, 
LINE, 
TEXT
FROM ALL_SOURCE 
WHERE 
    TYPE IN ('PROCEDURE', 'FUNCTION', 'PACKAGE', 'PACKAGE BODY') 
    AND OWNER NOT IN ('SYS', 'SYSMAN', 'CTXSYS', 'MDSYS', 'OLAPSYS', 'ORDSYS', 'OUTLN', 'WKSYS', 'WMSYS', 'XDB', 'ORDPLUGINS', 'SYSTEM')
    AND OWNER = :schemaOwner     
    AND (NAME = :name OR :name IS NULL)
ORDER BY OWNER, NAME, TYPE, LINE";

        }

        public IList<ProcedureSource> Execute(IConnectionAdapter connectionAdapter)
        {
            try
            {
                ExecuteDbReader(connectionAdapter);
            }
            catch (DbException exception)
            {
                //1. Security does not allow access
                Trace.TraceError("Handled: " + exception);
                //continue without the source
            }
            return Result;
        }

        protected override void AddParameters(DbCommand command)
        {
            EnsureOracleBindByName(command);
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "name", _name);
        }

        protected override void Mapper(IDataRecord record)
        {
            var name = record.GetString("Name");
            var schemaOwner = record.GetString("Owner");
            var type = record.GetString("Type").Trim();
            var sourceType = SourceType.StoredProcedure;
            switch (type)
            {
                case "PACKAGE": //oracle package
                    sourceType = SourceType.Package;
                    break;

                case "PACKAGE BODY": //oracle package body
                    sourceType = SourceType.PackageBody;
                    break;

                case "PROCEDURE": //oracle procedure
                    sourceType = SourceType.StoredProcedure;
                    break;

                case "FUNCTION": //oracle function
                    sourceType = SourceType.Function;
                    break;
            }
            var source = Result.Find(x => x.Name == name && x.SchemaOwner == schemaOwner && x.SourceType == sourceType);
            if (source == null)
            {
                source = new ProcedureSource
                {
                    Name = record.GetString("Name"),
                    SchemaOwner = record.GetString("Owner"),
                    SourceType = sourceType,
                };

                Result.Add(source);
            }

            //adding line by line
            //text will have a newline but not cReturn
            source.Text += record.GetString("Text");

        }
    }
}
