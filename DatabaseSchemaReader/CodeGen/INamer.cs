using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Translates table and column names to classes and properties.
    /// </summary>
    public interface INamer
    {
        /// <summary>
        /// Translates the namedObject's Name to a code-friendly name
        /// </summary>
        /// <param name="namedObject">The named object.</param>
        /// <returns></returns>
        string Name(NamedObject namedObject);

        /// <summary>
        /// Names the collection.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        string NameCollection(string className);

        /// <summary>
        /// For a column, returns the property name for a primary key
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        string PrimaryKeyName(DatabaseColumn column);

        /// <summary>
        /// Returns the name of a foreign key property for a given foreign key.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="foreignKey">The foreign key.</param>
        /// <returns></returns>
        string ForeignKeyName(DatabaseTable table, DatabaseConstraint foreignKey);

        /// <summary>
        /// Returns the name of an inverse foreign key property.
        /// </summary>
        /// <param name="targetTable">The target table.</param>
        /// <param name="table">The table.</param>
        /// <param name="foreignKey">The foreign key.</param>
        /// <returns></returns>
        string ForeignKeyCollectionName(string targetTable, DatabaseTable table, DatabaseConstraint foreignKey);
    }
}