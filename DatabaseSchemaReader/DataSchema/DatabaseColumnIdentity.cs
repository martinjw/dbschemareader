using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Identity properties for a database column
    /// </summary>
    [Serializable]
    public partial class DatabaseColumnIdentity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseColumnIdentity"/> class.
        /// </summary>
        public DatabaseColumnIdentity()
        {
            IdentitySeed = 1;
            IdentityIncrement = 1;
        }

        /// <summary>
        /// Gets or sets the seed value for an identity column (or equivalent)
        /// </summary>
        public long IdentitySeed { get; set; }

        /// <summary>
        /// Gets or sets the identity increment for an identity column (or equivalent)
        /// </summary>
        public long IdentityIncrement { get; set; }

        /// <summary>
        /// Gets or sets if the identity is "BY DEFAULT" (only incremented if INSERT statement does not specify a value). Default is false (i.e. "ALWAYS")
        /// </summary>
        public bool IdentityByDefault { get; set; }

        /// <summary>
        /// Get non-triviality of an identity column
        /// </summary>
        /// <returns>True if the identity sequence does not start at 1 and increment by 1.</returns>
        public bool IsNonTrivialIdentity()
        {
            return IdentitySeed != 1 || IdentityIncrement != 1;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Identity " + (IsNonTrivialIdentity() ? "(" + IdentitySeed + "," + IdentityIncrement + ")" : "");
        }
    }
}
