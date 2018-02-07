using System;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext
{
    class ConnectionAdapter : IConnectionAdapter
    {
        private readonly SchemaParameters _parameters;
        private DbConnection _dbConnection;

        public ConnectionAdapter(SchemaParameters parameters)
        {
            _parameters = parameters;
            _dbConnection = _parameters.DbConnection;
            DbTransaction = _parameters.DbTransaction;
        }

        public DbConnection DbConnection
        {
            get
            {
                if (_dbConnection == null)
                {
                    CreateDbConnection();
                }
                return _dbConnection;
            }
        }

        public DbTransaction DbTransaction { get; }

        private void CreateDbConnection()
        {
#if !COREFX
            System.Diagnostics.Trace.WriteLine($"Creating connection for {_parameters.ProviderName}");
            var factory = DatabaseSchemaReader.Utilities.DbProvider.FactoryTools.GetFactory(_parameters.ProviderName);
            _dbConnection = factory.CreateConnection();
            try
            {
                _dbConnection.ConnectionString = _parameters.ConnectionString;
            }
            catch (ArgumentException argumentException)
            {
                throw new InvalidOperationException("Invalid connection string " + _parameters.ConnectionString, argumentException);
            }
#endif
        }

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
#if !COREFX //in CoreFx, the connection is managed externally
            if (disposing)
            {
                // may have created it's own dbconnection
                if (_dbConnection != null && _dbConnection != _parameters.DbConnection)
                {
                    System.Diagnostics.Trace.WriteLine("Closing connection");
                    _dbConnection.Dispose();
                    _dbConnection = null;
                }
            }
#endif
        }

        #endregion Implementation of IDisposable
    }
}
