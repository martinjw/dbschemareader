using System;
using System.Linq;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    /// <summary>
    /// Use static <see cref="FindTrigger"/> to find an oracle autoNumber trigger.
    /// </summary>
    public class OracleSequenceTrigger
    {
        private OracleSequenceTrigger(DatabaseTrigger databaseTrigger)
        {
            DatabaseTrigger = databaseTrigger;
        }

        /// <summary>
        /// Finds the trigger that uses a sequence for autonumbering. May return NULL.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <returns></returns>
        public static OracleSequenceTrigger FindTrigger(DatabaseTable databaseTable)
        {
            var triggers = databaseTable.Triggers;
            if (triggers.Count == 0) return null;
            var pk = databaseTable.PrimaryKeyColumn ?? databaseTable.Columns.Find(x => x.IsAutoNumber);
            if (pk == null) return null;
            //the trigger body will look something like "SELECT MYSEQ.NEXTVAL INTO :NEW.ID FROM DUAL;"
            var pattern = ".NEXTVAL\\s+?INTO\\s+?:NEW.\"?" + pk.Name;
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            foreach (var databaseTrigger in databaseTable.Triggers)
            {
                var body = databaseTrigger.TriggerBody;
                var match = regex.Match(body);
                if (!match.Success) continue;
                var result = new OracleSequenceTrigger(databaseTrigger);

                //let's write the sequence if we can find it
                var seqName = ParseSequenceName(body, match.Index);
                if (seqName == null) return result;
                result.SequenceName = seqName;
                if (databaseTable.DatabaseSchema != null)
                {
                    result.DatabaseSequence = databaseTable.DatabaseSchema.Sequences
                        .FirstOrDefault(x => seqName.Equals(x.Name, StringComparison.OrdinalIgnoreCase));
                }
                return result;
            }

            return null;
        }

        private static string ParseSequenceName(string triggerBody, int startIndex)
        {
            //find the start of the .NEXTVAL... which should follow the sequence name
            //look back to where the sequence number begins
            var seqStart = triggerBody.Substring(0, startIndex).LastIndexOfAny(new[] { ' ', '\"', '.', '\n' });
            if (seqStart == -1) return null;
            var start = seqStart + 1;
            var length = startIndex - start;
            if (length == 0)
            {
                //the sequence name is quoted, so move back to the start quote
                seqStart = triggerBody.Substring(0, seqStart).LastIndexOf('\"');
                if (seqStart == -1) return null;
                start = seqStart + 1;
                length = (startIndex - start) - 1; //exclude the 
            }
            return triggerBody.Substring(start, length);
        }

        /// <summary>
        /// Gets the database trigger.
        /// </summary>
        /// <value>
        /// The database trigger.
        /// </value>
        public DatabaseTrigger DatabaseTrigger { get; private set; }

        /// <summary>
        /// Gets or sets the database sequence. Depending on trigger syntax, this may not be found.
        /// </summary>
        /// <value>
        /// The database sequence.
        /// </value>
        public DatabaseSequence DatabaseSequence { get; set; }

        /// <summary>
        /// Gets or sets the name of the sequence.
        /// </summary>
        /// <value>
        /// The name of the sequence.
        /// </value>
        public string SequenceName { get; set; }
    }
}
