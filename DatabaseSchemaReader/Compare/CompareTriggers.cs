using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareTriggers
    {
        private readonly StringBuilder _sb;
        private readonly ComparisonWriter _writer;

        public CompareTriggers(StringBuilder sb, ComparisonWriter writer)
        {
            _sb = sb;
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
                    _sb.AppendLine(_writer.DropTrigger(databaseTable, trigger));
                    continue;
                }
                if (trigger.TriggerBody != match.TriggerBody || trigger.TriggerType != match.TriggerType || trigger.TriggerEvent != match.TriggerEvent)
                {
                    _sb.AppendLine(_writer.DropTrigger(databaseTable, trigger));
                    _sb.AppendLine(_writer.AddTrigger(databaseTable, match));
                }
            }

            foreach (var trigger in secondTriggers)
            {
                var indexName = trigger.Name;
                var firstConstraint = firstTriggers.FirstOrDefault(c => c.Name == indexName);
                if (firstConstraint == null)
                {
                    _sb.AppendLine(_writer.AddTrigger(databaseTable, trigger));
                }
            }
        }
    }
}
