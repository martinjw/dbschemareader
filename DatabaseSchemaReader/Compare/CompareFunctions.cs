using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareFunctions
    {
        private readonly StringBuilder _sb;
        private readonly ComparisonWriter _writer;

        public CompareFunctions(StringBuilder sb, ComparisonWriter writer)
        {
            _sb = sb;
            _writer = writer;
        }

        public void Execute(IEnumerable<DatabaseFunction> baseFunctions, IEnumerable<DatabaseFunction> compareFunctions)
        {
            bool first = false;

            //find new functions (in compare, but not in base)
            foreach (var function in compareFunctions)
            {
                var name = function.Name;
                var schema = function.SchemaOwner;
                var match = baseFunctions.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                if (!first)
                {
                    first = true;
                    //CREATE FUNCTION cannot be combined with other statements in a batch, so be preceeded by and terminate with a "GO" (sqlServer) or "/" (Oracle)
                    if (_sb.Length > 0) _sb.AppendLine(_writer.RunStatements());
                }
                _sb.AppendLine("-- NEW FUNCTION " + function.Name);
                _sb.AppendLine(_writer.AddFunction(function));
            }

            //find dropped and existing functions
            foreach (var function in baseFunctions)
            {
                var name = function.Name;
                var schema = function.SchemaOwner;
                var match = compareFunctions.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    _sb.AppendLine("-- DROP FUNCTION " + function.Name);
                    _sb.AppendLine(_writer.DropFunction(function));
                    continue;
                }
                //functions may or may not have been changed

                if (function.Sql == match.Sql) continue; //the same
                //a sanitized comparison
                if (_writer.CompareProcedure(function.Sql, match.Sql)) continue;

                //in Oracle could be a CREATE OR REPLACE
                _sb.AppendLine("-- ALTER FUNCTION " + function.Name);
                _sb.AppendLine(_writer.DropFunction(function));
                _sb.AppendLine(_writer.AddFunction(match));
            }
        }
    }
}
