using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a schema in the database
    /// </summary>
    [Serializable]
    public partial class DatabaseDbSchema : NamedObject<DatabaseDbSchema>
    {
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }
    }
}