namespace DatabaseSchemaReader.Compare
{
    /// <summary>
    /// Result Types- change, add, delete
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        /// Changed (altered)
        /// </summary>
        Change,
        /// <summary>
        /// Added (created)
        /// </summary>
        Add,
        /// <summary>
        /// Deleted (dropped)
        /// </summary>
        Delete
    }
}