using System;
using System.Collections.Generic;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a package in the database (in Oracle, a collection of stored procedures and functions)
    /// </summary>
    [Serializable]
    public partial class DatabasePackage : NamedSchemaObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabasePackage"/> class.
        /// </summary>
        public DatabasePackage()
        {
            StoredProcedures = new List<DatabaseStoredProcedure>();
            Functions = new List<DatabaseFunction>();
        }

        /// <summary>
        /// Gets or sets the definition.
        /// </summary>
        /// <value>
        /// The definition.
        /// </value>
        public string Definition { get; set; }
        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        /// <value>
        /// The body.
        /// </value>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the stored procedures.
        /// </summary>
        /// <value>
        /// The stored procedures.
        /// </value>
        public List<DatabaseStoredProcedure> StoredProcedures { get; private set; }

        /// <summary>
        /// Gets or sets the functions.
        /// </summary>
        /// <value>
        /// The functions.
        /// </value>
        public List<DatabaseFunction> Functions { get; private set; }

        /// <summary>
        /// Gets or sets the name for a .Net class representing this specific package
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
