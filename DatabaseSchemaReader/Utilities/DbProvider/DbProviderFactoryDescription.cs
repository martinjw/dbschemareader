using System.Data;

namespace DatabaseSchemaReader.Utilities.DbProvider
{
    /// <summary>
    ///     Description of a DbProviderFactory for Repository.
    /// </summary>
    public class DbProviderFactoryDescription
    {

        /// <summary>
        ///     Initialize the description.
        /// </summary>
        public DbProviderFactoryDescription() {}

        /// <summary>
        ///     Initialize the description.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="invariant"></param>
        /// <param name="type"></param>
        public DbProviderFactoryDescription(string name, string description, string invariant, string type)
        {
            Name = name;
            Description = description;
            InvariantName = invariant;
            AssemblyQualifiedName = type;
        }

        /// <summary>
        ///     Initialize the description based on a row.
        /// </summary>
        /// <param name="row">The row.</param>
        internal DbProviderFactoryDescription(DataRow row)
        {
            Name = row["Name"].ToString();
            Description = row["Description"].ToString();
            InvariantName = row["InvariantName"].ToString();
            AssemblyQualifiedName = row["AssemblyQualifiedName"].ToString();
        }

        /// <summary>
        ///     Gets or sets the assemblyQualifiedName.
        /// </summary>
        /// <value>The assemblyQualifiedName.</value>
        public string AssemblyQualifiedName { get; set; }

        /// <summary>
        ///     Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the invariantName.
        /// </summary>
        /// <value>The invariantName.</value>
        public string InvariantName { get; set; }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return InvariantName;
        }
    }
}