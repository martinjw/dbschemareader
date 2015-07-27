namespace DatabaseSchemaReader.Filters
{
    /// <summary>
    /// Exclude specified items when reading schema
    /// </summary>
    public class Exclusions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Exclusions"/> class.
        /// </summary>
        public Exclusions()
        {
            TableFilter = new Filter();
            ViewFilter = new Filter();
            StoredProcedureFilter = new Filter();
            PackageFilter = new Filter();
        }
        /// <summary>
        /// Gets or sets the table filter.
        /// </summary>
        /// <value>
        /// The table filter.
        /// </value>
        public IFilter TableFilter { get; set; }
        /// <summary>
        /// Gets or sets the view filter.
        /// </summary>
        /// <value>
        /// The view filter.
        /// </value>
        public IFilter ViewFilter { get; set; }
        /// <summary>
        /// Gets or sets the stored procedure filter.
        /// </summary>
        /// <value>
        /// The stored procedure filter.
        /// </value>
        public IFilter StoredProcedureFilter { get; set; }
        /// <summary>
        /// Gets or sets the package filter.
        /// </summary>
        /// <value>
        /// The package filter.
        /// </value>
        public IFilter PackageFilter { get; set; }
    }
}
