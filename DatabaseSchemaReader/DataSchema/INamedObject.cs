namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A named database object
    /// </summary>
    public interface INamedObject
    {
        /// <summary>
        /// Gets or sets the name (original database format)
        /// </summary>
        /// <value>
        /// The table name.
        /// </value>
        string Name { get; set; }
    }
}