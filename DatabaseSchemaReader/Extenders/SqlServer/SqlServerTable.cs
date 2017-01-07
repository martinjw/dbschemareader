using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Extenders.SqlServer
{
    /// <summary>
    /// A cutsomized Sql Server table
    /// </summary>
    /// <seealso cref="DatabaseSchemaReader.DataSchema.DatabaseTable" />
    public class SqlServerTable : DatabaseTable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerTable"/> class.
        /// </summary>
        public SqlServerTable()
        {
            DatabaseStatistics = new List<DatabaseStatistics>();
        }

        /// <summary>
        /// Gets the database statistics.
        /// </summary>
        /// <value>
        /// The database statistics.
        /// </value>
        public List<DatabaseStatistics> DatabaseStatistics { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this table is memory optimized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this table is memory optimized; otherwise, <c>false</c>.
        /// </value>
        public bool IsMemoryOptimized { get; set; }
    }
}