using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareConstraints
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareConstraints(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(DatabaseTable baseTable, DatabaseTable compareTable)
        {
            ComparePrimaryKey(baseTable, compareTable);
            Compare(baseTable, baseTable.UniqueKeys, compareTable.UniqueKeys);
            Compare(baseTable, baseTable.CheckConstraints, compareTable.CheckConstraints);
            Compare(baseTable, baseTable.ForeignKeys, compareTable.ForeignKeys);
        }


        private void Compare(DatabaseTable databaseTable,
            ICollection<DatabaseConstraint> firstConstraints,
            ICollection<DatabaseConstraint> secondConstraints)
        {
            foreach (var constraint in firstConstraints)
            {
                var constraintName = constraint.Name;
                var matchConstraint = secondConstraints.FirstOrDefault(c => c.Name == constraintName);
                if (matchConstraint == null)
                {
                    CreateResult(ResultType.Delete, databaseTable, constraintName,
                        _writer.DropConstraint(databaseTable, constraint));
                    continue;
                }
                if (!ConstraintColumnsEqual(constraint, matchConstraint))
                {
                    CreateResult(ResultType.Change, databaseTable, constraintName,
                        _writer.DropConstraint(databaseTable, constraint) + Environment.NewLine +
                        _writer.AddConstraint(databaseTable, matchConstraint));
                    continue;
                }
                if (constraint.ConstraintType == ConstraintType.Check &&
                    constraint.Expression != matchConstraint.Expression)
                {
                    CreateResult(ResultType.Change, databaseTable, constraintName,
                        _writer.DropConstraint(databaseTable, constraint) + Environment.NewLine +
                        _writer.AddConstraint(databaseTable, matchConstraint));
                }
                if (constraint.ConstraintType == ConstraintType.ForeignKey &&
                    constraint.RefersToTable != matchConstraint.RefersToTable)
                {
                    //unlikely a foreign key will change the fk table without changing name
                    CreateResult(ResultType.Change, databaseTable, constraintName,
                       _writer.DropConstraint(databaseTable, constraint) + Environment.NewLine +
                       _writer.AddConstraint(databaseTable, matchConstraint));
                }


            }
            foreach (var constraint in secondConstraints)
            {
                var constraintName = constraint.Name;
                var firstConstraint = firstConstraints.FirstOrDefault(c => c.Name == constraintName);
                if (firstConstraint == null)
                {
                    CreateResult(ResultType.Add, databaseTable, constraintName,
                        _writer.AddConstraint(databaseTable, constraint));
                }
            }
        }

        private void ComparePrimaryKey(DatabaseTable databaseTable, DatabaseTable match)
        {
            if (databaseTable.PrimaryKey == null && match.PrimaryKey == null)
            {
                //no primary key before or after. Oh dear.
                Trace.TraceWarning("-- NB: " + databaseTable.Name + " has no primary key!");
                return;
            }
            if (databaseTable.PrimaryKey == null)
            {
                //forgot to put pk on it
                CreateResult(ResultType.Add, databaseTable, match.PrimaryKey.Name,
                    _writer.AddConstraint(databaseTable, match.PrimaryKey));
            }
            else if (match.PrimaryKey == null)
            {
                //why oh why would you want to drop the primary key?
                CreateResult(ResultType.Change, databaseTable, databaseTable.PrimaryKey.Name,
                    _writer.DropConstraint(databaseTable, databaseTable.PrimaryKey) + Environment.NewLine +
                    "-- NB: " + databaseTable.Name + " has no primary key!");
            }
            else if (!ConstraintColumnsEqual(databaseTable.PrimaryKey, match.PrimaryKey))
            {
                CreateResult(ResultType.Change, databaseTable, databaseTable.PrimaryKey.Name,
                    _writer.DropConstraint(databaseTable, databaseTable.PrimaryKey) + Environment.NewLine +
                    _writer.AddConstraint(match, match.PrimaryKey));
            }
        }

        private static bool ConstraintColumnsEqual(DatabaseConstraint first, DatabaseConstraint second)
        {
            if (first.Columns == null && second.Columns == null) return true; //same, both null
            if (first.Columns == null || second.Columns == null) return false; //one is null, they are different
            return first.Columns.SequenceEqual(second.Columns);
        }


        private void CreateResult(ResultType resultType, DatabaseTable table, string name, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.Constraint,
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
