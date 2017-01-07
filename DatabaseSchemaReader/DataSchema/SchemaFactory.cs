namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Factory to create schema objects. Override this to create specific objects with additional properties.
    /// </summary>
    public class SchemaFactory
    {
        /// <summary>
        /// Creates the database table.
        /// </summary>
        /// <returns></returns>
        public virtual DatabaseTable CreateDatabaseTable()
        {
            return new DatabaseTable();
        }
    }
}
