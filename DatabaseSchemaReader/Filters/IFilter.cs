namespace DatabaseSchemaReader.Filters
{
    /// <summary>
    /// Exclude specified items when readings schema
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// Excludes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        bool Exclude(string name);
    }
}