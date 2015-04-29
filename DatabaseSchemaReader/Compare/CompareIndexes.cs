using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareIndexes
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareIndexes(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(DatabaseTable databaseTable, DatabaseTable compareTable)
        {
            var firstIndexes = databaseTable.Indexes;
            var secondIndexes = compareTable.Indexes;
            foreach (var index in firstIndexes)
            {
                if (index.IsUniqueKeyIndex(databaseTable)) continue;

                var indexName = index.Name;
                var match = secondIndexes.FirstOrDefault(c => c.Name == indexName);
                if (match == null)
                {
                    CreateResult(ResultType.Delete, databaseTable, indexName,
                        _writer.DropIndex(databaseTable, index));
                    continue;
                }
                if (!ColumnsEqual(index, match) || (index.IndexType != match.IndexType))
                {
                    CreateResult(ResultType.Add, databaseTable, indexName,
                       _writer.DropIndex(databaseTable, index) + Environment.NewLine +
                       _writer.AddIndex(databaseTable, match));
                }
            }

            foreach (var index in secondIndexes)
            {
                if (index.IsUniqueKeyIndex(compareTable)) continue;

                var indexName = index.Name;
                var firstConstraint = firstIndexes.FirstOrDefault(c => c.Name == indexName);
                if (firstConstraint == null)
                {
                    CreateResult(ResultType.Add, databaseTable, indexName,
                        _writer.AddIndex(databaseTable, index));
                }
            }
        }

        private static bool ColumnsEqual(DatabaseIndex first, DatabaseIndex second)
        {
            if (first.Columns == null && second.Columns == null) return true; //same, both null
            if (first.Columns == null || second.Columns == null) return false; //one is null, they are different
            //the two sequences have the same names
            var columnNames1 = first.Columns.OrderBy(c => c.Ordinal).Select(c => c.Name);
            var columnNames2 = second.Columns.OrderBy(c => c.Ordinal).Select(c => c.Name);

            return columnNames1.SequenceEqual(columnNames2);
        }


        private void CreateResult(ResultType resultType, DatabaseTable table, string name, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.Index,
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
