using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareFunctions
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareFunctions(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(ICollection<DatabaseFunction> baseFunctions, ICollection<DatabaseFunction> compareFunctions)
        {
            bool first = false;

            //find new functions (in compare, but not in base)
            foreach (var function in compareFunctions)
            {
                var name = function.Name;
                var schema = function.SchemaOwner;
                var match = baseFunctions.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                var script = string.Empty;
                if (!first)
                {
                    first = true;
                    //CREATE FUNCTION cannot be combined with other statements in a batch, 
                    //so be preceeded by and terminate with a "GO" (sqlServer) or "/" (Oracle)
                    if (_results.Count > 0) script+=_writer.RunStatements() + Environment.NewLine;
                }
                script+="-- NEW FUNCTION " + function.Name + Environment.NewLine +
                    _writer.AddFunction(function);
                CreateResult(ResultType.Add, function, script);
            }

            //find dropped and existing functions
            foreach (var function in baseFunctions)
            {
                var name = function.Name;
                var schema = function.SchemaOwner;
                var match = compareFunctions.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    CreateResult(ResultType.Delete, function, "-- DROP FUNCTION " + function.Name + Environment.NewLine +
                        _writer.DropFunction(function));
                    continue;
                }
                //functions may or may not have been changed

                if (function.Sql == match.Sql) continue; //the same
                //a sanitized comparison
                if (_writer.CompareProcedure(function.Sql, match.Sql)) continue;

                //in Oracle could be a CREATE OR REPLACE
                CreateResult(ResultType.Change, function, "-- ALTER FUNCTION " + function.Name + Environment.NewLine +
                    _writer.DropFunction(function) + Environment.NewLine +
                    _writer.AddFunction(match));
            }
        }


        private void CreateResult(ResultType resultType, DatabaseFunction function, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.Function,
                    ResultType = resultType,
                    Name = function.Name,
                    SchemaOwner = function.SchemaOwner,
                    Script = script
                };
            _results.Add(result);
        }
    }
}
