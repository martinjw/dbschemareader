using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a user in the database
    /// </summary>
    [Serializable]
    public class DatabaseUser : NamedObject<DatabaseUser>
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
