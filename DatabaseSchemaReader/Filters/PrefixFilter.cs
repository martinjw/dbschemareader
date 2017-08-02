﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseSchemaReader.Filters
{
    /// <summary>
    /// Exclude items when reading the schema
    /// </summary>
    public class PrefixFilter : IFilter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrefixFilter"/> class.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        public PrefixFilter(params string[] prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));

            foreach (var s in prefix)
            {
                FilterExclusions.Add(s);
            }
        }

        /// <summary>
        /// An exclusions list.
        /// </summary>
        /// <value>
        /// The exclusions.
        /// </value>
        public IList<string> FilterExclusions { get; } = new List<string>();

        /// <summary>
        /// Excludes the specified name with any of the prefixes.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual bool Exclude(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return FilterExclusions
                .Any(x => name.StartsWith(x, StringComparison.OrdinalIgnoreCase));
        }
    }
}