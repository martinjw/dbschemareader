using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareTables
    {
        private readonly StringBuilder _sb;
        private readonly ComparisonWriter _writer;

        public CompareTables(StringBuilder sb, ComparisonWriter writer)
        {
            _sb = sb;
            _writer = writer;
        }

        public void Execute(IEnumerable<DatabaseTable> baseTables, IEnumerable<DatabaseTable> compareTables)
        {
            //find new tables (in compare, but not in base)
            var newTables = new List<DatabaseTable>();

            foreach (var databaseTable in compareTables)
            {
                var name = databaseTable.Name;
                var schema = databaseTable.SchemaOwner;
                var match = baseTables.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                _sb.AppendLine("-- NEW TABLE " + databaseTable.Name);
                _sb.AppendLine(_writer.AddTable(databaseTable));
                newTables.Add(databaseTable);
            }


            //find dropped and existing tables
            foreach (var databaseTable in baseTables)
            {
                var name = databaseTable.Name;
                var schema = databaseTable.SchemaOwner;
                var match = compareTables.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    _sb.AppendLine(_writer.DropTable(databaseTable));
                    continue;
                }
                //table may or may not have been changed

                //add, alter and delete columns
                var compareColumns = new CompareColumns(_sb, _writer);
                compareColumns.Execute(databaseTable, match);

                //add, alter and delete constraints
                var compareConstraints = new CompareConstraints(_sb, _writer);
                compareConstraints.Execute(databaseTable, match);

                //indexes
                var compareIndexes = new CompareIndexes(_sb, _writer);
                compareIndexes.Execute(databaseTable, match);

                //triggers
                var compareTriggers = new CompareTriggers(_sb, _writer);
                compareTriggers.Execute(databaseTable, match);
            }


            //add tables doesn't add foreign key constraints (wait until all tables created)
            foreach (var databaseTable in newTables)
            {
                foreach (var foreignKey in databaseTable.ForeignKeys)
                {
                    _sb.AppendLine(_writer.AddConstraint(databaseTable, foreignKey));
                }
                foreach (var trigger in databaseTable.Triggers)
                {
                    _sb.AppendLine(_writer.AddTrigger(databaseTable, trigger));
                }
            }
        }

    }
}
