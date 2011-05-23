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

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseStoredProcedure"/> class.
        /// </summary>
        public DatabaseStoredProcedure()
        {
            _arguments = new List<DatabaseArgument>();
            _resultSets = new List<DatabaseResultSet>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
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

        /// <summary>
        /// Gets or sets the schema owner.
        /// </summary>
        /// <value>
        /// The schema owner.
        /// </value>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Gets or sets the package.
        /// </summary>
        /// <value>
        /// The package.
        /// </value>
        public string Package { get; set; }

        /// <summary>
        /// Gets or sets the body SQL.
        /// </summary>
        /// <value>
        /// The body SQL.
        /// </value>
        public string Sql { get; set; }

        /// <summary>
        /// Gets or sets the language (for instance, PostgreSql).
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the return type (otherwise check arguments[0]).
        /// </summary>
        /// <value>
        /// The return type.
        /// </value>
        public string ReturnType { get; set; }

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        public List<DatabaseArgument> Arguments { get { return _arguments; } }
        /// <summary>
        /// Gets the result sets.
        /// </summary>
        public List<DatabaseResultSet> ResultSets { get { return _resultSets; } }

        /// <summary>
        /// Gets or sets the database schema.
        /// </summary>
        /// <value>
        /// The database schema.
        /// </value>
        public DatabaseSchema DatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the name in .Net format (for a class representing the specific procedure)
        /// </summary>
        /// <value>
        /// The name of the net.
        /// </value>
        public string NetName { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

    }
}
