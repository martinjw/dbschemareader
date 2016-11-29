namespace DatabaseSchemaReader.Filters
{
    /// <summary>
    /// Include (rather than exclude)
    /// </summary>
    /// <seealso cref="DatabaseSchemaReader.Filters.Filter" />
    public class InclusionFilter : Filter
    {
        /// <summary>
        /// Excludes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public override bool Exclude(string name)
        {
            //inverted, so exclusion is really inclusion
            return !FilterExclusions.Contains(name);
        }
    }
}
