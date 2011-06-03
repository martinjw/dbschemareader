
using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a sequence in the database (eg Oracle, SqlServer 2011)
    /// </summary>
    [Serializable]
    public partial class DatabaseSequence
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the schema owner.
        /// </summary>
        /// <value>
        /// The schema owner.
        /// </value>
        public string SchemaOwner { get; set; }
        /// <summary>
        /// Gets or sets the mininum value.
        /// </summary>
        /// <value>
        /// The mininum value.
        /// </value>
        public decimal? MinimumValue { get; set; }
        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>
        /// The maximum value.
        /// </value>
        public decimal? MaximumValue { get; set; }
        /// <summary>
        /// Gets or sets the increment by.
        /// </summary>
        /// <value>
        /// The increment by.
        /// </value>
        public int IncrementBy { get; set; }

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
