using System.Collections.Generic;

namespace DatabaseSchemaReader.Filters
{
    /// <summary>
    /// Exclude specified items when readings schema
    /// </summary>
    public interface IFilter
    {
        /// <summary>
        /// An exclusions list. The implementation may use simple names, regex strings, or not use this list at all.
        /// </summary>
        /// <value>
        /// The exclusions.
        /// </value>
        IList<string> FilterExclusions { get; }

        /// <summary>
        /// Excludes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        bool Exclude(string name);
    }
}