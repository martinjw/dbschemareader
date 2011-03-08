using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    [Serializable]
    public class DatabaseStoredProcedure
    {
        #region Fields
        //backing fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseArgument> _arguments;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseResultSet> _resultSets;
        #endregion

        public DatabaseStoredProcedure()
        {
            _arguments = new List<DatabaseArgument>();
            _resultSets = new List<DatabaseResultSet>();
        }

        public string Name { get; set; }

        public string SchemaOwner { get; set; }

        public string Package { get; set; }

        public string Sql { get; set; }

        public List<DatabaseArgument> Arguments { get { return _arguments; } }
        public List<DatabaseResultSet> ResultSets { get { return _resultSets; } }

        public DatabaseSchema DatabaseSchema { get; set; }

        public string NetName { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }
}
