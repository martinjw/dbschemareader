using System.Collections.Generic;

namespace DatabaseSchemaReader.Filters
{
    /// <summary>
    /// Exclude items when reading the schema
    /// </summary>
    public class Filter : IFilter
    {
        private readonly IList<string> _filterExclusions = new List<string>();

        /// <summary>
        /// An exclusions list. The implementation may use simple names, regex strings, or not use this list at all.
        /// </summary>
        /// <value>
        /// The exclusions.
        /// </value>
        public IList<string> FilterExclusions
        {
            get { return _filterExclusions; }
        }


        /// <summary>
        /// Excludes the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual bool Exclude(string name)
        {
            //could use linq for case insensitive
            if (FilterExclusions.Contains(name)) return true;
            return false;
        }
    }
}
