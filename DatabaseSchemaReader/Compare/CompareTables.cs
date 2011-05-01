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
            foreach (var databaseTable in compareTables)
            {
                var name = databaseTable.Name;
                var schema = databaseTable.SchemaOwner;
                var match = baseTables.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                _sb.AppendLine("-- NEW TABLE " + databaseTable.Name);
                _sb.AppendLine(_writer.WriteTable(databaseTable));
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
                ComparePrimaryKey(databaseTable, match);
                CompareConstraints(databaseTable, databaseTable.UniqueKeys, match.UniqueKeys);
                CompareConstraints(databaseTable, databaseTable.CheckConstraints, match.CheckConstraints);
                CompareConstraints(databaseTable, databaseTable.ForeignKeys, match.ForeignKeys);

                //indexes
            }
        }

        private void CompareConstraints(DatabaseTable databaseTable, 
            IEnumerable<DatabaseConstraint> firstConstraints, 
            IEnumerable<DatabaseConstraint> secondConstraints)
        {
            foreach (var constraint in firstConstraints)
            {
                var constraintName = constraint.Name;
                var matchConstraint = secondConstraints.FirstOrDefault(c => c.Name == constraintName);
                if (matchConstraint == null)
                {
                    _sb.AppendLine(_writer.DropConstraint(databaseTable, constraint));
                    continue;
                }
                if (!ConstraintColumnsEqual(constraint, matchConstraint))
                {
                    _sb.AppendLine(_writer.DropConstraint(databaseTable, constraint));
                    _sb.AppendLine(_writer.AddConstraint(databaseTable, matchConstraint));
                    continue;
                }
                if (constraint.ConstraintType == ConstraintType.Check &&
                    constraint.Expression != matchConstraint.Expression)
                {
                    _sb.AppendLine(_writer.DropConstraint(databaseTable, constraint));
                    _sb.AppendLine(_writer.AddConstraint(databaseTable, matchConstraint));
                }
                if (constraint.ConstraintType == ConstraintType.ForeignKey &&
                    constraint.RefersToTable != matchConstraint.RefersToTable)
                {
                    //unlikely a foreign key will change the fk table without changing name
                    _sb.AppendLine(_writer.DropConstraint(databaseTable, constraint));
                    _sb.AppendLine(_writer.AddConstraint(databaseTable, matchConstraint));
                }


            }
            foreach (var constraint in secondConstraints)
            {
                var constraintName = constraint.Name;
                var firstConstraint = firstConstraints.FirstOrDefault(c => c.Name == constraintName);
                if (firstConstraint == null)
                {
                    _sb.AppendLine(_writer.AddConstraint(databaseTable, constraint));
                }
            }
        }

        private void ComparePrimaryKey(DatabaseTable databaseTable, DatabaseTable match)
        {
            if (databaseTable.PrimaryKey == null && match.PrimaryKey == null)
            {
                //no primary key before or after. Oh dear.
                _sb.AppendLine("-- NB: " + databaseTable.Name + " has no primary key!");
                return;
            }  
            if (databaseTable.PrimaryKey == null)
            {
                //forgot to put pk on it
                _sb.AppendLine(_writer.AddConstraint(databaseTable, match.PrimaryKey));
            }
            else if (match.PrimaryKey == null)
            {
                //why oh why would you want to drop the primary key?
                _sb.AppendLine(_writer.DropConstraint(databaseTable, databaseTable.PrimaryKey));
                _sb.AppendLine("-- NB: " + databaseTable.Name + " has no primary key!");
            }
            else if (!ConstraintColumnsEqual(databaseTable.PrimaryKey, match.PrimaryKey))
            {
                _sb.AppendLine(_writer.DropConstraint(databaseTable, databaseTable.PrimaryKey));
                _sb.AppendLine(_writer.AddConstraint(databaseTable, match.PrimaryKey));
            }
        }

        private static bool ConstraintColumnsEqual(DatabaseConstraint first, DatabaseConstraint second)
        {
            if (first.Columns == null && second.Columns == null) return true; //same, both null
            if (first.Columns == null || second.Columns == null) return false; //one is null, they are different
            return first.Columns.SequenceEqual(second.Columns);
        }

    }
}
