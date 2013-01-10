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
    }
}