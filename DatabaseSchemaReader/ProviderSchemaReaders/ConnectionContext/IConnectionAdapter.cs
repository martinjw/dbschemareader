using System;
using System.Data.Common;

namespace DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext
{
    /// <summary>
    /// Container for DbConnection and transaction
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface IConnectionAdapter : IDisposable
    {
        /// <summary>
        /// Gets the database connection.
        /// </summary>
        /// <value>
        /// The database connection.
        /// </value>
        DbConnection DbConnection { get; }
        /// <summary>
        /// Gets the database transaction.
        /// </summary>
        /// <value>
        /// The database transaction.
        /// </value>
        DbTransaction DbTransaction { get; }
    }
}