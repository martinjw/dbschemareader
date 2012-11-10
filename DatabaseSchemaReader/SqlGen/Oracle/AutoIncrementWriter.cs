using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Oracle
{
    /// <summary>
    /// Turn a column defined as IDENTITY into Oracle Sequence/Trigger
    /// </summary>
    class AutoIncrementWriter
    {
        private readonly DatabaseTable _table;

        public AutoIncrementWriter(DatabaseTable table)
        {
            _table = table;
        }

        public string Write()
        {
            if (!_table.HasIdentityColumn) return null;

            var txt = WriteExistingTrigger();
            if (txt != null) return txt;


            var identityColumn = _table.Columns.First(x => x.IsIdentity).Name;

            string sequenceName = _table.Name + "_SEQUENCE";
            int i = 0;
            var schema = _table.DatabaseSchema;
            if (schema != null)
            {
                while (FindSequenceName(schema, sequenceName))
                {
                    i++;
                    sequenceName = _table.Name + "_SEQUENCE" + i;
                }
            }

            string triggerName = _table.Name + "_PK_TRIGGER";
            i = 0;
            while (FindTriggerName(triggerName))
            {
                i++;
                triggerName = _table.Name + "_PK_TRIGGER" + i;
            }

            var sb = new StringBuilder();
            sb.AppendLine("-- sequence for " + _table.Name);
            sb.AppendLine(WriteSequence(sequenceName));
            sb.AppendLine();
            sb.AppendLine("-- auto-increment trigger for " + _table.Name);
            sb.AppendLine(WriteTrigger(triggerName, sequenceName, identityColumn));
            return sb.ToString();
        }

        private string WriteExistingTrigger()
        {
            //does it already have a trigger which sets the sequence? In which case, note it...
            var pk = _table.PrimaryKeyColumn;
            var pattern = ".NEXTVAL\\s+?INTO\\s+?:NEW.\"?" + pk.Name;
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (var databaseTrigger in _table.Triggers)
            {
                var body = databaseTrigger.TriggerBody;
                var match = regex.Match(body);
                if (match.Success)
                {
                    var generator = new OracleMigrationGenerator();
                    var sb = new StringBuilder();
                    if (_table.DatabaseSchema != null)
                    {
                        //let's write the sequence if we can find it
                        var startPos = match.Index;
                        var seqStart = body.Substring(0, startPos).LastIndexOfAny(new[] { ' ', '\"', '.', '\n' });
                        if (seqStart != -1)
                        {
                            var start = seqStart + 1;
                            var length = startPos - start;
                            var seqName = body.Substring(start, length);
                            var seq = _table.DatabaseSchema.Sequences
                                .FirstOrDefault(x => seqName.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                            if (seq != null)
                            {
                                sb.AppendLine(generator.AddSequence(seq));
                            }
                        }

                    }

                    sb.AppendLine("-- use auto-increment trigger " + databaseTrigger.Name);
                    sb.Append(generator.AddTrigger(_table, databaseTrigger));
                    return sb.ToString();
                }
            }
            //nothing found
            return null;
        }

        private static bool FindSequenceName(DatabaseSchema schema, string sequenceName)
        {
            return schema.Sequences.Any(seq => seq.Name.Equals(sequenceName, StringComparison.OrdinalIgnoreCase));
        }

        private bool FindTriggerName(string triggerName)
        {
            return _table.Triggers.Any(trigger => trigger.Name.Equals(triggerName, StringComparison.OrdinalIgnoreCase));
        }

        private static string WriteSequence(string sequenceName)
        {
            const string sequence = @"CREATE SEQUENCE {0} 
INCREMENT BY 1 
START WITH 1 
MINVALUE 1;";
            return string.Format(CultureInfo.InvariantCulture,
                                 sequence,
                                 sequenceName);
        }

        private string WriteTrigger(string triggerName, string sequenceName, string identityColumn)
        {
            const string trigger = @"CREATE OR REPLACE TRIGGER ""{0}"".""{1}"" 
BEFORE INSERT ON ""{0}"".""{2}""    
FOR EACH ROW BEGIN
    IF INSERTING THEN
        IF :NEW.""{3}"" IS NULL THEN
            SELECT {4}.NEXTVAL INTO :NEW.""{3}"" FROM DUAL;
        END IF;
    END IF;
END;
/
";

            return string.Format(
                CultureInfo.InvariantCulture,
                trigger,
                _table.SchemaOwner,
                triggerName,
                _table.Name,
                identityColumn,
                sequenceName
                );
        }
    }
}
