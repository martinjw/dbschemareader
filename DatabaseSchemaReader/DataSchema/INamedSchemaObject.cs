namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A database object with name and schema
    /// </summary>
    /// <seealso cref="DatabaseSchemaReader.DataSchema.INamedObject" />
    public interface INamedSchemaObject : INamedObject
    {

        /// <summary>
        /// Gets or sets the schema/owner.
        /// </summary>
        /// <value>
        /// The schema/owner.
        /// </value>
        string SchemaOwner { get; set; }
    }
}