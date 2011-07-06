using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class ComparePackages
    {
        private readonly StringBuilder _sb;
        private readonly ComparisonWriter _writer;

        public ComparePackages(StringBuilder sb, ComparisonWriter writer)
        {
            _sb = sb;
            _writer = writer;
        }

        public void Execute(IEnumerable<DatabasePackage> basePackages, IEnumerable<DatabasePackage> comparePackages)
        {
            bool first = false;

            //find new packages (in compare, but not in base)
            foreach (var package in comparePackages)
            {
                var name = package.Name;
                var schema = package.SchemaOwner;
                var match = basePackages.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                if (!first)
                {
                    first = true;
                    //CREATE PACKAGE cannot be combined with other statements in a batch, so be preceeded by and terminate with a  "/"
                    if (_sb.Length > 0) _sb.AppendLine(_writer.RunStatements());
                }

                _sb.AppendLine("-- NEW PACKAGE " + package.Name);
                _sb.AppendLine(_writer.AddPackage(package));
            }

            //find dropped and existing packages
            foreach (var package in basePackages)
            {
                var name = package.Name;
                var schema = package.SchemaOwner;
                var match = comparePackages.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    _sb.AppendLine("-- DROP PACKAGE " + package.Name);
                    _sb.AppendLine(_writer.DropPackage(package));
                    continue;
                }

                if (package.Body == match.Body && package.Definition == match.Definition) continue;

                if (!first)
                {
                    first = true;
                    //CREATE PACKAGE cannot be combined with other statements in a batch, so be preceeded by and terminate with a  "/"
                    if (_sb.Length > 0) _sb.AppendLine(_writer.RunStatements());
                }

                //different package
                _sb.AppendLine("-- ALTER PACKAGE " + package.Name);
                //we rely on CREATE OR REPLACE here (no drop!)
                _sb.AppendLine(_writer.AddPackage(match));
            }
        }
    }
}
