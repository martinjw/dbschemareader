using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareColumns
    {
        private readonly StringBuilder _sb;
        private readonly ComparisonWriter _writer;

        public CompareColumns(StringBuilder sb, ComparisonWriter writer)
        {
            _sb = sb;
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
                _sb.AppendLine("-- ADDED TABLE " + column.TableName + " COLUMN " + name);
                _sb.AppendLine(_writer.AddColumn(compareTable, column));
            }

            //find dropped and existing columns
            foreach (var column in baseTable.Columns)
            {
                var name = column.Name;
                var match = compareTable.Columns.FirstOrDefault(t => t.Name == name);
                if (match == null)
                {
                    _sb.AppendLine(_writer.DropColumn(baseTable, column));
                    continue;
                }

                //has column changed?

                if (column.DbDataType == match.DbDataType && 
                    column.Length == match.Length &&
                    column.Precision == match.Precision && 
                    column.Scale == match.Scale &&
                    column.Nullable == match.Nullable)
                {
                    //we don't check IDENTITY
                    continue; //the same, no action
                }

                _sb.AppendLine(_writer.AlterColumn(baseTable, match, column));
            }
        }

    }
}
