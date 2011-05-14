using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Compare
{
    class CompareSequences
    {
        private readonly StringBuilder _sb;
        private readonly ComparisonWriter _writer;

        public CompareSequences(StringBuilder sb, ComparisonWriter writer)
        {
            _sb = sb;
            _writer = writer;
        }

        public void Execute(IEnumerable<DatabaseSequence> baseSequences, IEnumerable<DatabaseSequence> compareSequences)
        {
            //find new sequences (in compare, but not in base)
            foreach (var sequence in compareSequences)
            {
                var name = sequence.Name;
                var schema = sequence.SchemaOwner;
                var match = baseSequences.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match != null) continue;
                _sb.AppendLine("-- NEW SEQUENCE " + sequence.Name);
                _sb.AppendLine(_writer.AddSequence(sequence));
            }

            //find dropped and existing sequence
            foreach (var sequence in baseSequences)
            {
                var name = sequence.Name;
                var schema = sequence.SchemaOwner;
                var match = compareSequences.FirstOrDefault(t => t.Name == name && t.SchemaOwner == schema);
                if (match == null)
                {
                    _sb.AppendLine("-- DROP SEQUENCE " + sequence.Name);
                    _sb.AppendLine(_writer.DropSequence(sequence));
                    continue;
                }

                //we could alter the sequence, but it's rare you'd ever want to do this
            }
        }
    }
}
