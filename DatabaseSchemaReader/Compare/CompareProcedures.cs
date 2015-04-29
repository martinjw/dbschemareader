using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareProcedures
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareProcedures(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(ICollection<DatabaseStoredProcedure> baseProcedures, ICollection<DatabaseStoredProcedure> compareProcedures)
        {
            bool first = false;

            //find new sprocs (in compare, but not in base)
            foreach (var procedure in compareProcedures)
            {
                var name = procedure.Name;
                var schema = procedure.SchemaOwner;
                var match = baseProcedures.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                var script = string.Empty;
                if (!first)
                {
                    first = true;
                    //CREATE PROCEDURE cannot be combined with other statements in a batch, 
                    //so be preceeded by and terminate with a "GO" (sqlServer) or "/" (Oracle)
                    if (_results.Count > 0) script+=_writer.RunStatements() + Environment.NewLine;
                }
                script+="-- NEW STORED PROCEDURE " + procedure.Name + Environment.NewLine +
                    _writer.AddProcedure(procedure);
                CreateResult(ResultType.Add, procedure, script);
            }

            //find dropped and existing sprocs
            foreach (var procedure in baseProcedures)
            {
                var name = procedure.Name;
                var schema = procedure.SchemaOwner;
                var match = compareProcedures.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    CreateResult(ResultType.Delete, procedure,
                                 "-- DROP STORED PROCEDURE " + procedure.Name + Environment.NewLine +
                                 _writer.DropProcedure(procedure));
                    continue;
                }
                //sproc may or may not have been changed

                if (procedure.Sql == match.Sql) continue; //the same
                //a sanitized comparison
                if (_writer.CompareProcedure(procedure.Sql, match.Sql)) continue;

                //in Oracle could be a CREATE OR REPLACE
                CreateResult(ResultType.Change, procedure, 
                    "-- ALTER STORED PROCEDURE " + procedure.Name + Environment.NewLine +
                    _writer.DropProcedure(procedure) + Environment.NewLine +
                    _writer.AddProcedure(match));
            }
        }


        private void CreateResult(ResultType resultType, DatabaseStoredProcedure storedProcedure, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.StoredProcedure,
                    ResultType = resultType,
                    Name = storedProcedure.Name,
                    SchemaOwner = storedProcedure.SchemaOwner,
                    Script = script
                };
            _results.Add(result);
        }
    }
}
