using DatabaseSchemaReader;
using SqlServerSchemaReader.Extenders;
using SqlServerSchemaReader.Schema;

namespace SqlServerSchemaReader
{
    /// <summary>
    /// An example customized reader for SqlServer
    /// </summary>
    /// <seealso cref="DatabaseSchemaReader.DatabaseReader" />
    public class SqlServerDatabaseReader : DatabaseReader
    {
#if COREFX
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDatabaseReader"/> class from a DbConnection.
        /// </summary>
        /// <param name="connection">The connection. Should be a SqlConnection.</param>
        public SqlServerDatabaseReader(System.Data.Common.DbConnection connection) : base(connection)
        {
            AddExtenders();
        }
#endif
#if !COREFX

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerDatabaseReader"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SqlServerDatabaseReader(string connectionString)
            : base(new SqlServerSchema(connectionString))
        {
            AddExtenders();
        }

#endif

        private void AddExtenders()
        {
            AddTableExtender(new SqlServerTableExtender());
            AddSchemaExtender(new SqlServerSchemaExtender());
            AddSchemaFactory(new SqlServerSchemaFactory());
        }
    }
}
