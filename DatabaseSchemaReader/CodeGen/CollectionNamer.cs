namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Names collections by adding "Collection" at the end. Or use <see cref="PluralizingNamer"/>
    /// </summary>
    public class CollectionNamer : ICollectionNamer
    {
        /// <summary>
        /// Names the collection.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public string NameCollection(string className)
        {
            return className + "Collection";
        }
    }
}
