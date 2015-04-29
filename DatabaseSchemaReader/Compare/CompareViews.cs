using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareViews
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareViews(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
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
                var script = string.Empty;
                if (!first)
                {
                    first = true;
                    //CREATE VIEW cannot be combined with other statements in a batch, so be preceeded by and terminate with a "GO" (sqlServer) or "/" (Oracle)
                    if (_results.Count > 0) script += _writer.RunStatements() + Environment.NewLine;
                }
                script += "-- NEW VIEW " + view.Name + Environment.NewLine +
                 _writer.AddView(view);
                CreateResult(ResultType.Add, view, script);
            }

            //find dropped and existing views
            foreach (var view in baseViews)
            {
                var name = view.Name;
                var schema = view.SchemaOwner;
                var match = compareViews.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    CreateResult(ResultType.Delete, view,
                        "-- DROP VIEW " + view.Name + Environment.NewLine +
                        _writer.DropView(view));
                    continue;
                }
                //view may or may not have been changed

                //we require the view Sql (otherwise we can't write it)
                if (view.Sql == match.Sql) continue; //the same
                //a sanitized comparison
                if (_writer.CompareView(view.Sql, match.Sql)) continue;

                //in Oracle could be a CREATE OR REPLACE
                var script = "-- ALTER VIEW " + view.Name + Environment.NewLine +
                    _writer.DropView(view) + Environment.NewLine +
                    _writer.AddView(match);
                CreateResult(ResultType.Change, view, script);
            }
        }

        private void CreateResult(ResultType resultType, DatabaseView view, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.View,
                    ResultType = resultType,
                    Name = view.Name,
                    SchemaOwner = view.SchemaOwner,
                    Script = script
                };
            _results.Add(result);
        }
    }
}
