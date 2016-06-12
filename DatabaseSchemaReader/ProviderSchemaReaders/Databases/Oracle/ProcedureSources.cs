using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using DatabaseSchemaReader.ProviderSchemaReaders.ResultModels;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    internal class ProcedureSources : OracleSqlExecuter<ProcedureSource>
    {
        private readonly string _name;

        public ProcedureSources(string owner, string name)
        {
            _name = name;
            Owner = owner;
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
    AND OWNER = :schemaOwner AND    
    AND (NAME = :name OR :name IS NULL)
ORDER BY OWNER, NAME, TYPE, LINE";

        }

        public IList<ProcedureSource> Execute(DbConnection connection)
        {
            try
            {
                ExecuteDbReader(connection);
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
            var source = Result.Find(x => x.Name == name && x.SchemaOwner == schemaOwner);
            if (source == null)
            {
                source = new ProcedureSource
                {
                    Name = record.GetString("Name"),
                    SchemaOwner = record.GetString("Owner")
                };
                switch (record.GetString("Type").Trim())
                {
                    case "PACKAGE": //oracle package
                        source.SourceType = SourceType.Package;
                        break;

                    case "PACKAGE BODY": //oracle package body
                        source.SourceType = SourceType.PackageBody;
                        break;

                    case "PROCEDURE": //oracle procedure
                        source.SourceType = SourceType.StoredProcedure;
                        break;

                    case "FUNCTION": //oracle function
                        source.SourceType = SourceType.Function;
                        break;
                }
            }

            //adding line by line
            //text will have a newline but not cReturn
            source.Text += record.GetString("Text");
            Result.Add(source);

        }
    }
}
