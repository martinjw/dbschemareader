using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a trigger in the database.
    /// </summary>
    [Serializable]
    public partial class DatabaseTrigger
    {
        /// <summary>
        /// Gets or sets the database schema.
        /// </summary>
        /// <value>
        /// The database schema.
        /// </value>
        public DatabaseSchema DatabaseSchema { get; set; }

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
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the trigger body.
        /// </summary>
        /// <value>
        /// The trigger body.
        /// </value>
        public string TriggerBody { get; set; }

        /// <summary>
        /// Gets or sets the trigger event.
        /// </summary>
        /// <value>
        /// The trigger event.
        /// </value>
        public string TriggerEvent { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name + " on " + TableName;
        }
    }
}
