using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class ComparePackages
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public ComparePackages(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(ICollection<DatabasePackage> basePackages, ICollection<DatabasePackage> comparePackages)
        {
            bool first = false;

            //find new packages (in compare, but not in base)
            foreach (var package in comparePackages)
            {
                var name = package.Name;
                var schema = package.SchemaOwner;
                var match = basePackages.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                var script = string.Empty;
                if (!first)
                {
                    first = true;
                    //CREATE PACKAGE cannot be combined with other statements in a batch, 
                    //so be preceeded by and terminate with a  "/"
                    if (_results.Count > 0) script += _writer.RunStatements() + Environment.NewLine;
                }

                script += "-- NEW PACKAGE " + package.Name + Environment.NewLine +
                    _writer.AddPackage(package);
                CreateResult(ResultType.Add, package, script);
            }

            //find dropped and existing packages
            foreach (var package in basePackages)
            {
                var name = package.Name;
                var schema = package.SchemaOwner;
                var match = comparePackages.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    CreateResult(ResultType.Delete, package, "-- DROP PACKAGE " + package.Name + Environment.NewLine +
                        _writer.DropPackage(package));
                    continue;
                }

                if (package.Body == match.Body && package.Definition == match.Definition) continue;

                var script = string.Empty;
                if (!first)
                {
                    first = true;
                    //CREATE PACKAGE cannot be combined with other statements in a batch, 
                    //so be preceeded by and terminate with a  "/"
                    if (_results.Count > 0) script += _writer.RunStatements() + Environment.NewLine;
                }

                //different package
                script += "-- ALTER PACKAGE " + package.Name + Environment.NewLine;
                //we rely on CREATE OR REPLACE here (no drop!)
                script += _writer.AddPackage(match);
                CreateResult(ResultType.Delete, package, script);
            }
        }

        private void CreateResult(ResultType resultType, DatabasePackage package, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.Package,
                    ResultType = resultType,
                    Name = package.Name,
                    SchemaOwner = package.SchemaOwner,
                    Script = script
                };
            _results.Add(result);
        }
    }
}
