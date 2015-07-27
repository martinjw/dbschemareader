using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a view in the database
    /// </summary>
    /// <remarks>
    /// Essentially the same (and inherits from) a <see cref="DatabaseTable"/>
    /// </remarks>
    [Serializable]
    public partial class DatabaseView : DatabaseTable
    {
        /// <summary>
        /// Gets or sets the DDL to create this view (if available)
        /// </summary>
        /// <value>The SQL.</value>
        public string Sql { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "View: " + base.ToString();
        }
    }


}
