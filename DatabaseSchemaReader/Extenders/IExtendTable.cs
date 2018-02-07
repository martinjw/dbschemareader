using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.Extenders
{
    /// <summary>
    /// Extend a database table
    /// </summary>
    public interface IExtendTable
    {
        /// <summary>
        /// Add additional information to database table.
        /// </summary>
        void Execute(DatabaseTable databaseTable, IConnectionAdapter connection);
    }
}