using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents an index in the database
    /// </summary>
    [Serializable]
    public partial class DatabaseIndex
    {
        #region Fields
        //backing fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly List<DatabaseColumn> _columns;
        #endregion


        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseIndex"/> class.
        /// </summary>
        public DatabaseIndex()
        {
            _columns = new List<DatabaseColumn>();
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the schema owner.
        /// </summary>
        /// <value>
        /// The schema owner.
        /// </value>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the type of the index.
        /// </summary>
        /// <value>
        /// The type of the index.
        /// </value>
        public string IndexType { get; set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        public List<DatabaseColumn> Columns { get { return _columns; } }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name + " on " + TableName;
        }

    }
}
