using System;
using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle;

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
            if (!_table.HasAutoNumberColumn) return null;

            var txt = WriteExistingTrigger();
            if (txt != null) return txt;

            var autoNumberColumn = _table.Columns.First(x => x.IsAutoNumber);
            if (autoNumberColumn.IdentityDefinition != null)
            {
                if (_table.DatabaseSchema != null &&
                    _table.DatabaseSchema.Provider.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    //this is an Oracle schema with identity, so assume it's Oracle 12+ identity
                    return null;
                }
            }
            if (ProviderSchemaReaders.Databases.Oracle.Conversion.LooksLikeAutoNumberDefaults(autoNumberColumn.DefaultValue))
            {
                return null;
            }
            var identityColumn = autoNumberColumn.Name;

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
            var result = OracleSequenceTrigger.FindTrigger(_table);
            if (result == null) return null;
            var generator = new OracleMigrationGenerator();
            var sb = new StringBuilder();
            if (result.DatabaseSequence != null)
            {
                sb.AppendLine(generator.AddSequence(result.DatabaseSequence));
            }
            sb.AppendLine("-- use auto-increment trigger " + result.DatabaseTrigger.Name);
            sb.Append(generator.AddTrigger(_table, result.DatabaseTrigger));
            return sb.ToString();
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
            const string sequence = @"CREATE SEQUENCE {0};";
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
                )
                //if we empty the schema (using default) don't add an empty "".
                .Replace("\"\".", "");
        }
    }
}
