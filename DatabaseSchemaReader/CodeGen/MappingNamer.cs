using System.Collections.Generic;

namespace DatabaseSchemaReader.CodeGen
{
    class MappingNamer
    {
        public MappingNamer()
        {
            EntityNames = new List<string>();
        }

        /// <summary>
        /// Gets or sets the entity names.
        /// </summary>
        /// <value>
        /// The entity names.
        /// </value>
        public IList<string> EntityNames { get; private set; }

        /// <summary>
        /// Names the mapping class for an entity
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <returns></returns>
        public string NameMappingClass(string entityName)
        {
            var className = entityName + "Mapping";
            if (EntityNames != null)
            {
                //resolve any name conflicts
                while (EntityNames.Contains(className))
                {
                    className += "Map";
                }
                EntityNames.Add(className);
            }
            return className;
        }

    }
}
