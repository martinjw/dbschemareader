using System; 
namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a sequence in the database (eg Oracle, SqlServer 2011)
    /// </summary>
    [Serializable]
    public partial class DatabaseSequence : NamedSchemaObject<DatabaseSequence>
    {
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>
        /// The minimum value.
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
        /// Gets or sets the increment by. May be 0 if not defined.
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
