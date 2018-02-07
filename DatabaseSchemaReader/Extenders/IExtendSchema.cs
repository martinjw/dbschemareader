using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.Extenders
{
    /// <summary>
    /// Extend a database schema
    /// </summary>
    public interface IExtendSchema
    {
        /// <summary>
        /// Add additional information to database schema.
        /// </summary>
        void Execute(DatabaseSchema databaseSchema, IConnectionAdapter connection);
    }
}