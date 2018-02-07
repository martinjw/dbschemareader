using System;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext
{
    class NestedConnection : IConnectionAdapter
    {
        public NestedConnection(IConnectionAdapter connectionAdapter)
        {
            DbConnection = connectionAdapter.DbConnection;
            DbTransaction = connectionAdapter.DbTransaction;
        }

        public DbConnection DbConnection { get; }
        public DbTransaction DbTransaction { get; }

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
            //don't release the connection because we're nested
        }

        #endregion Implementation of IDisposable
    }
}