using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a view in the database
    /// </summary>
    [Serializable]
    public class DatabaseView : DatabaseTable
    {
        /// <summary>
        /// Gets or sets the DDL to create this view (if available)
        /// </summary>
        /// <value>The SQL.</value>
        public string Sql { get; set; }
        
        public override string ToString()
        {
            return "View: " + base.ToString();
        }
    }


}
