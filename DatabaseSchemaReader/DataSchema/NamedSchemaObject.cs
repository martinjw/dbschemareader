using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A database object that has an unique schema and name
    /// </summary>
    [Serializable]
    public abstract class NamedSchemaObject<T> : NamedObject<T>, INamedSchemaObject where T : NamedSchemaObject<T>
    {

        /// <summary>
        /// Gets or sets the schema owner.
        /// </summary>
        /// <value>
        /// The schema owner.
        /// </value>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public override bool Equals(T other)
        {
            if (other == null)
            {
                return false;
            }
            if (Name == null && other.Name == null) return Equals(this, other);
            return (Name == other.Name && SchemaOwner == other.SchemaOwner);
        }

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

            var o = (NamedSchemaObject<T>)obj;
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
