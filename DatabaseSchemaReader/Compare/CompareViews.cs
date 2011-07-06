using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareViews
    {
        private readonly StringBuilder _sb;
        private readonly ComparisonWriter _writer;

        public CompareViews(StringBuilder sb, ComparisonWriter writer)
        {
            _sb = sb;
            _writer = writer;
        }

        public void Execute(IEnumerable<DatabaseView> baseViews, IEnumerable<DatabaseView> compareViews)
        {
            bool first = false;

            //find new views (in compare, but not in base)
            foreach (var view in compareViews)
            {
                var name = view.Name;
                var schema = view.SchemaOwner;
                var match = baseViews.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                if (!first)
                {
                    first = true;
                    //CREATE VIEW cannot be combined with other statements in a batch, so be preceeded by and terminate with a "GO" (sqlServer) or "/" (Oracle)
                    if (_sb.Length > 0) _sb.AppendLine(_writer.RunStatements());
                }
                _sb.AppendLine("-- NEW VIEW " + view.Name);
                _sb.AppendLine(_writer.AddView(view));
            }

            //find dropped and existing views
            foreach (var view in baseViews)
            {
                var name = view.Name;
                var schema = view.SchemaOwner;
                var match = compareViews.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    _sb.AppendLine("-- DROP VIEW " + view.Name);
                    _sb.AppendLine(_writer.DropView(view));
                    continue;
                }
                //view may or may not have been changed

                //we require the view Sql (otherwise we can't write it)
                if (view.Sql == match.Sql) continue; //the same
                //a sanitized comparison
                if (_writer.CompareView(view.Sql, match.Sql)) continue;

                //in Oracle could be a CREATE OR REPLACE
                _sb.AppendLine("-- ALTER VIEW " + view.Name);
                _sb.AppendLine(_writer.DropView(view));
                _sb.AppendLine(_writer.AddView(match));
            }
        }
    }
}
