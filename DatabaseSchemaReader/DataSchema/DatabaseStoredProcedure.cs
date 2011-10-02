using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a stored procedure in the database.
    /// </summary>
    [Serializable]
    public partial class DatabaseStoredProcedure : NamedSchemaObject
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
                if (!string.IsNullOrEmpty(SchemaOwner) && !SchemaOwner.Equals("dbo"))
                {
                    //prefix with schema name
                    fullName = SchemaOwner + "." + fullName;
                }
                return fullName;
            }
        }

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

        #region overrides
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

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        ///   </exception>
        public override bool Equals(object obj)
        {
            var x = obj as DatabaseStoredProcedure;
            if (x == null) return false;
            return string.Equals(FullName, x.FullName);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(FullName)) return base.GetHashCode();
            return FullName.GetHashCode();
        }
        #endregion


    }
}
