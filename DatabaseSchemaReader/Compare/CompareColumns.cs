﻿using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;

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
            var copy = baseTable.Clone();
            foreach (var column in compareTable.Columns)
            {
                var name = column.Name;
                var match = baseTable.Columns.FirstOrDefault(t => t.Name == name);
                if (match != null) continue;
                var script = "-- ADDED TABLE " + column.TableName + " COLUMN " + name + Environment.NewLine +
                 _writer.AddColumn(compareTable, column);
                copy.AddColumn(column);
                CreateResult(ResultType.Add, baseTable, name, script);
            }

            //find dropped and existing columns
            var toDrop = new Dictionary<string, DatabaseColumn>();
            var toAlter = new Dictionary<string, DatabaseColumn[]>();
            foreach (var column in baseTable.Columns)
            {
                var name = column.Name;
                var match = compareTable.Columns.FirstOrDefault(t => t.Name == name);
                if (match == null)
                {
                    toDrop.Add(name, column);
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
                toAlter.Add(name, new[] { match, column });
            }

            //write drops and alters as last step to ensure valid queries
            foreach (var kv in toAlter)
            {
                copy.Columns.Remove(kv.Value[1]);
                copy.Columns.Add(kv.Value[0]);
                CreateResult(ResultType.Change, baseTable, kv.Key,
                    _writer.AlterColumn(copy, kv.Value[0], kv.Value[1]));
            }

            foreach (var kv in toDrop)
            {
                copy.Columns.Remove(kv.Value);
                CreateResult(ResultType.Delete, baseTable, kv.Key,
                    _writer.DropColumn(copy, kv.Value));
                continue;
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