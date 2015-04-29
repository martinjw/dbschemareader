using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareTables
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareTables(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
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
                var script = "-- NEW TABLE " + databaseTable.Name + Environment.NewLine +
                    _writer.AddTable(databaseTable);
                CreateResult(ResultType.Add, databaseTable, script);
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
                    CreateResult(ResultType.Delete, databaseTable, _writer.DropTable(databaseTable));
                    continue;
                }
                //table may or may not have been changed

                //add, alter and delete columns
                var compareColumns = new CompareColumns(_results, _writer);
                compareColumns.Execute(databaseTable, match);

                //add, alter and delete constraints
                var compareConstraints = new CompareConstraints(_results, _writer);
                compareConstraints.Execute(databaseTable, match);

                //indexes
                var compareIndexes = new CompareIndexes(_results, _writer);
                compareIndexes.Execute(databaseTable, match);

                //triggers
                var compareTriggers = new CompareTriggers(_results, _writer);
                compareTriggers.Execute(databaseTable, match);
            }


            //add tables doesn't add foreign key constraints (wait until all tables created)
            foreach (var databaseTable in newTables)
            {
                foreach (var foreignKey in databaseTable.ForeignKeys)
                {
                    var result = new CompareResult
                    {
                        SchemaObjectType = SchemaObjectType.Constraint,
                        ResultType = ResultType.Add,
                        SchemaOwner = databaseTable.SchemaOwner,
                        TableName = databaseTable.Name,
                        Name = foreignKey.Name,
                        Script = _writer.AddConstraint(databaseTable, foreignKey)
                    };
                    _results.Add(result);
                }
                foreach (var trigger in databaseTable.Triggers)
                {
                    var result = new CompareResult
                    {
                        SchemaObjectType = SchemaObjectType.Trigger,
                        ResultType = ResultType.Add,
                        Name = trigger.Name,
                        SchemaOwner = databaseTable.SchemaOwner,
                        TableName = databaseTable.Name,
                        Script = _writer.AddTrigger(databaseTable, trigger)
                    };
                    _results.Add(result);
                }
            }
        }

        private void CreateResult(ResultType resultType, DatabaseTable table, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.Table,
                    ResultType = resultType,
                    Name = table.Name,
                    SchemaOwner = table.SchemaOwner,
                    Script = script
                };
            _results.Add(result);
        }
    }
}
