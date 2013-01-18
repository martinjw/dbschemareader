using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Translates table and column names to classes and properties.
    /// </summary>
    public class Namer : ICollectionNamer, INamer
    {
        /// <summary>
        /// Translates the namedObject's Name to a code-friendly name
        /// </summary>
        /// <param name="namedObject">The named object.</param>
        /// <returns></returns>
        public virtual string Name(NamedObject namedObject)
        {
            var name = NameFixer.ToPascalCase(namedObject.Name);
            var column = namedObject as DatabaseColumn;
            if (column != null)
            {
                //if it's a foreign key (CategoryId)
                if (column.IsForeignKey && name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    //remove the "Id" - it's just a "Category"
                    name = name.Substring(0, name.Length - 2);
                }
                //member name cannot be same as class name
                if (name == column.Table.NetName)
                {
                    name += "Property";
                }
            }
            return name;
        }

        /// <summary>
        /// Names the collection.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns></returns>
        public virtual string NameCollection(string className)
        {
            return className + "Collection";
        }
    }
}
