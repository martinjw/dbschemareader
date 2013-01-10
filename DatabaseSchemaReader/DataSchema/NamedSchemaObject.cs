using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A database object that has an unique schema and name
    /// </summary>
    [Serializable]
    public abstract class NamedSchemaObject : NamedObject
    {


        /// <summary>
        /// Gets or sets the schema owner.
        /// </summary>
        /// <value>
        /// The schema owner.
        /// </value>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var o = (NamedSchemaObject)obj;
            if (Name == null && o.Name == null) return base.Equals(obj);
            return (Name == o.Name && SchemaOwner == o.SchemaOwner);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (string.IsNullOrEmpty(Name)) return base.GetHashCode(); //transient instance
            var hashCode = Name.GetHashCode();
            //a schema is optional, but if it has one bitwise exclusive-OR
            if (!string.IsNullOrEmpty(SchemaOwner)) hashCode ^= SchemaOwner.GetHashCode();
            return hashCode;
        }

    }
}
