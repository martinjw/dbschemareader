using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareTriggers
    {
        private readonly IList<CompareResult> _results;
        private readonly ComparisonWriter _writer;

        public CompareTriggers(IList<CompareResult> results, ComparisonWriter writer)
        {
            _results = results;
            _writer = writer;
        }

        public void Execute(DatabaseTable databaseTable, DatabaseTable compareTable)
        {
            var firstTriggers = databaseTable.Triggers;
            var secondTriggers = compareTable.Triggers;
            foreach (var trigger in firstTriggers)
            {
                var indexName = trigger.Name;
                var match = secondTriggers.FirstOrDefault(c => c.Name == indexName);
                if (match == null)
                {
                    CreateResult(ResultType.Delete, databaseTable, indexName, 
                        _writer.DropTrigger(databaseTable, trigger));
                    continue;
                }
                if (trigger.TriggerBody != match.TriggerBody || 
                    trigger.TriggerType != match.TriggerType || 
                    trigger.TriggerEvent != match.TriggerEvent)
                {
                    CreateResult(ResultType.Change, databaseTable, indexName,
                        _writer.DropTrigger(databaseTable, trigger) + Environment.NewLine +
                        _writer.AddTrigger(databaseTable, match));
                }
            }

            foreach (var trigger in secondTriggers)
            {
                var indexName = trigger.Name;
                var firstConstraint = firstTriggers.FirstOrDefault(c => c.Name == indexName);
                if (firstConstraint == null)
                {
                    CreateResult(ResultType.Add, databaseTable, indexName, 
                        _writer.AddTrigger(databaseTable, trigger));
                }
            }
        }

        private void CreateResult(ResultType resultType, DatabaseTable table, string name, string script)
        {
            var result = new CompareResult
                {
                    SchemaObjectType = SchemaObjectType.Trigger,
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
