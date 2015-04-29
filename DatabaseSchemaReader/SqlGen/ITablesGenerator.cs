namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Generate Ddl for all tables in schema.
    /// </summary>
    public interface ITablesGenerator
    {
        /// <summary>
        /// Indicates whether schema will be written in DDL
        /// </summary>
        /// <value><c>true</c> if schema is written; otherwise, <c>false</c>.</value>
        bool IncludeSchema { get; set; }

        /// <summary>
        /// Writes this ddl script.
        /// </summary>
        /// <returns></returns>
        string Write();
    }
}