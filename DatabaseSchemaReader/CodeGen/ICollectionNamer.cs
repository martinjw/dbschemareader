namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Name the collections
    /// </summary>
    public interface ICollectionNamer
    {
        /// <summary>
        /// Names the collection.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        string NameCollection(string className);
    }
}