using System.Collections.Generic;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a result set of a stored procedure
    /// </summary>
    public class DatabaseResultSet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseResultSet"/> class.
        /// </summary>
        public DatabaseResultSet()
        {
            Columns = new List<DatabaseColumn>();
        }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        public IList<DatabaseColumn> Columns { get; private set; }
    }
}
