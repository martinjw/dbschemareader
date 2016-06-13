using System;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Filters;


namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class SchemaParameters : IDisposable
    {
        private bool _createdConnection;
#if NETSTANDARD1_5
        public SchemaParameters(DbConnection dbConnection)
        {
            DbConnection = dbConnection;
            ProviderName = DbConnection.GetType().Namespace;
            ConnectionString = dbConnection.ConnectionString;
            SqlType = ProviderToSqlType.Convert(ProviderName);
            Exclusions = new Exclusions();
        }

#else
        private void CreateDbConnection()
        {
            var factory = DatabaseSchemaReader.Utilities.DbProvider.FactoryTools.GetFactory(ProviderName);
            DbConnection = factory.CreateConnection();
            try
            {
                DbConnection.ConnectionString = ConnectionString;
            }
            catch (ArgumentException argumentException)
            {
                throw new InvalidOperationException("Invalid connection string "+ ConnectionString, argumentException);
            }
            _createdConnection = true;
        }

        public SchemaParameters(string connectionString, SqlType sqlType)
        {
            ConnectionString = connectionString;
            ProviderName = ProviderToSqlType.Convert(sqlType);
            SqlType = sqlType;
            Exclusions = new Exclusions();
            CreateDbConnection();
        }

        public SchemaParameters(string connectionString, string provider)
        {
            ConnectionString = connectionString;
            ProviderName = provider;
            SqlType = ProviderToSqlType.Convert(ProviderName);
            Exclusions = new Exclusions();
            CreateDbConnection();
        }
#endif

        public string ConnectionString { get; private set; }

        public DbConnection DbConnection { get; private set; }
        public SqlType? SqlType { get; private set; }

        /// <summary>
        /// Exclude specified items when reading schema
        /// </summary>
        /// <value>
        /// The exclusions.
        /// </value>
        public Exclusions Exclusions { get; private set; }

        /// <summary>
        /// Gets or sets the owner (for Oracle) /schema (for SqlServer) / database (MySql). Always set it with Oracle; if you use other than dbo in SqlServer you should also set it.
        /// If it is null or empty, all owners are returned.
        /// </summary>
        public string Owner { get; set; }

        public string ProviderName { get; private set; }

        public DatabaseSchema DatabaseSchema { get; set; }


#region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_createdConnection)
                {
                    DbConnection.Dispose();
                }
            }
        }

#endregion Implementation of IDisposable
    }
}
