using System;
using System.Linq;

namespace DatabaseSchemaReader.Filters
{
    /// <summary>
    /// Include items when reading the schema
    /// </summary>
    public class InclusionPrefixFilter : PrefixFilter
    {
        /// <summary>
        /// Include tables with these prefixes
        /// </summary>
        /// <param name="prefix">The prefixes</param>
        public InclusionPrefixFilter(params string[] prefix) : base(prefix)
        {
        }

        /// <summary>
        /// Include tables with names starting with the specified prefixes
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public override bool Exclude(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return !FilterExclusions
                .Any(x => name.StartsWith(x, StringComparison.OrdinalIgnoreCase));
        }
    }
}