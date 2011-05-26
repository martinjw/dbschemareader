using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareIndexes
    {
        private readonly StringBuilder _sb;
        private readonly ComparisonWriter _writer;

        public CompareIndexes(StringBuilder sb, ComparisonWriter writer)
        {
            _sb = sb;
            _writer = writer;
        }

        public void Execute(DatabaseTable databaseTable, DatabaseTable compareTable)
        {
            var firstIndexes = databaseTable.Indexes;
            var secondIndexes = compareTable.Indexes;
            foreach (var index in firstIndexes)
            {
                if(index.IsUniqueKeyIndex(databaseTable)) continue;

                var indexName = index.Name;
                var match = secondIndexes.FirstOrDefault(c => c.Name == indexName);
                if (match == null)
                {
                    _sb.AppendLine(_writer.DropIndex(databaseTable, index));
                    continue;
                }
                if (!ColumnsEqual(index, match) || (index.IndexType != match.IndexType))
                {
                    _sb.AppendLine(_writer.DropIndex(databaseTable, index));
                    _sb.AppendLine(_writer.AddIndex(databaseTable, match));
                    continue;
                }
            }

            foreach (var index in secondIndexes)
            {
                if (index.IsUniqueKeyIndex(compareTable)) continue;

                var indexName = index.Name;
                var firstConstraint = firstIndexes.FirstOrDefault(c => c.Name == indexName);
                if (firstConstraint == null)
                {
                    _sb.AppendLine(_writer.AddIndex(databaseTable, index));
                }
            }
        }

        private static bool ColumnsEqual(DatabaseIndex first, DatabaseIndex second)
        {
            if (first.Columns == null && second.Columns == null) return true; //same, both null
            if (first.Columns == null || second.Columns == null) return false; //one is null, they are different
            //the two sequences have the same names
            var columnNames1 = first.Columns.OrderBy(c=> c.Ordinal).Select(c => c.Name);
            var columnNames2 = second.Columns.OrderBy(c => c.Ordinal).Select(c => c.Name);

            return columnNames1.SequenceEqual(columnNames2);
        }

    }
}
