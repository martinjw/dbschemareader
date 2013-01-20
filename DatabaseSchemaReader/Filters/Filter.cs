namespace DatabaseSchemaReader.Filters
{
    /// <summary>
    /// Exclude items when reading the schema
    /// </summary>
    public class Filter : IFilter
    {
        /// <summary>
        /// Excludes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual bool Exclude(string name)
        {
            return false;
        }
    }
}
