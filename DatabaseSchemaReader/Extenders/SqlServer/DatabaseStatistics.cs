using System;
using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Extenders.SqlServer
{
    /// <summary>
    /// SqlServerStatistics
    /// </summary>
    [Serializable]
    public partial class DatabaseStatistics : NamedSchemaObject<DatabaseStatistics>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseIndex"/> class.
        /// </summary>
        public DatabaseStatistics()
        {
            Columns = new List<string>();
        }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets the columns of the statistics.
        /// </summary>
        public List<string> Columns { get; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name + " in " + TableName;
        }
    }
}