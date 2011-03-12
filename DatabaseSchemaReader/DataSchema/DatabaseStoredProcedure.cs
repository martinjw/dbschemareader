using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a stored procedure in the database.
    /// </summary>
    [Serializable]
    public partial class DatabaseStoredProcedure
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

        /// <summary>
        /// Gets the full name (including schema if applicable, and package if applicable)
        /// </summary>
        public string FullName
        {
            get
            {
                var fullName = Name;
                if (!string.IsNullOrEmpty(Package))
                {
                    //prefix with package name
                    fullName = Package + "." + fullName;
                }
                if (!SchemaOwner.Equals("dbo"))
                {
                    //prefix with schema name
                    fullName = SchemaOwner + "." + fullName;
                }
                return fullName;
            }
        }

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
