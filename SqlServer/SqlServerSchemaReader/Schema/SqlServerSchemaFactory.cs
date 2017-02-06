using DatabaseSchemaReader.DataSchema;

namespace SqlServerSchemaReader.Schema
{
    /// <summary>
    /// Factory to create schema objects like tables
    /// </summary>
    /// <seealso cref="SchemaFactory" />
    public class SqlServerSchemaFactory : SchemaFactory
    {
        /// <summary>
        /// Creates a SqlServer table.
        /// </summary>
        /// <returns></returns>
        public override DatabaseTable CreateDatabaseTable()
        {
            return new SqlServerTable();
        }
    }
}