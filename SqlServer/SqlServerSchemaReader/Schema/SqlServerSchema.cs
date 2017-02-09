using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;
using System.Linq;

namespace SqlServerSchemaReader.Schema
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
            TableTypes = new List<TableType>();
            AliasTypes = new List<AliasType>();
        }

        /// <summary>
        /// Gets the SQL server tables.
        /// </summary>
        /// <value>
        /// The SQL server tables.
        /// </value>
        public IEnumerable<SqlServerTable> SqlServerTables { get { return Tables.OfType<SqlServerTable>(); } }

        /// <summary>
        ///User Defined table types.
        /// </summary>
        public List<TableType> TableTypes { get; set; }

        /// <summary>
        /// User Defined data types.
        /// </summary>
        public List<AliasType> AliasTypes { get; set; }
    }
}