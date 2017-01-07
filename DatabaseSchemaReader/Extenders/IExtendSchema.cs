using System.Data.Common;
using DatabaseSchemaReader.DataSchema;

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
        void Execute(DatabaseSchema databaseSchema, DbConnection connection);
    }
}