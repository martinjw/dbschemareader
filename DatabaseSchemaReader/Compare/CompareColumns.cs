using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareColumns
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareColumns(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(DatabaseTable baseTable, DatabaseTable compareTable)
        {
            //find new columns (in compare, but not in base)
            foreach (var column in compareTable.Columns)
            {
                var name = column.Name;
                var match = baseTable.Columns.FirstOrDefault(t => t.Name == name);
                if (match != null) continue;
                var script = "-- ADDED TABLE " + column.TableName + " COLUMN " + name + Environment.NewLine +
                 _writer.AddColumn(compareTable, column);
                CreateResult(ResultType.Add, baseTable, name, script);
            }

            //find dropped and existing columns
            foreach (var column in baseTable.Columns)
            {
                var name = column.Name;
                var match = compareTable.Columns.FirstOrDefault(t => t.Name == name);
                if (match == null)
                {
                    CreateResult(ResultType.Delete, baseTable, name,
                        _writer.DropColumn(baseTable, column));
                    continue;
                }

                //has column changed?

                if (string.Equals(column.DbDataType, match.DbDataType, StringComparison.OrdinalIgnoreCase) &&
                    column.Length == match.Length &&
                    column.Precision == match.Precision &&
                    column.Scale == match.Scale &&
                    column.Nullable == match.Nullable)
                {
                    //we don't check IDENTITY
                    continue; //the same, no action
                }

                CreateResult(ResultType.Change, baseTable, name,
                    _writer.AlterColumn(baseTable, match, column));
            }
        }


        private void CreateResult(ResultType resultType, DatabaseTable table, string name, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.Column,
                    ResultType = resultType,
                    TableName = table.Name,
                    SchemaOwner = table.SchemaOwner,
                    Name = name,
                    Script = script
                };
            _results.Add(result);
        }
    }
}
