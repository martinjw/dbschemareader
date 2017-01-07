using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Extenders.SqlServer
{
    /// <summary>
    /// A customized sql server schema
    /// </summary>
    /// <seealso cref="DatabaseSchemaReader.DataSchema.DatabaseSchema" />
    public class SqlServerSchema : DatabaseSchema
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerSchema"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqlServerSchema(string connectionString) : base(connectionString, "System.Data.SqlClient")
        {
        }

        /// <summary>
        /// Gets the SQL server tables.
        /// </summary>
        /// <value>
        /// The SQL server tables.
        /// </value>
        public IEnumerable<SqlServerTable> SqlServerTables { get { return Tables.OfType<SqlServerTable>(); } }
    }
}