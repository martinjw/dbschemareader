using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a result set of a stored procedure
    /// </summary>
    [Serializable]
    public partial class DatabaseResultSet
    {
        #region Fields
        //backing fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseColumn> _columns;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseResultSet"/> class.
        /// </summary>
        public DatabaseResultSet()
        {
            _columns = new List<DatabaseColumn>();
        }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        public List<DatabaseColumn> Columns { get { return _columns; } }

        /// <summary>
        /// Gets or sets the result set name in .Net (C#) compatible format.
        /// </summary>
        /// <value>
        /// The .net name
        /// </value>
        public string NetName { get; set; }
    }
}
