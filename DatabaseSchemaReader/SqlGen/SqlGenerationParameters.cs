namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Properties to control the sql generation
    /// </summary>
    public class SqlGenerationParameters
    {
        /// <summary>
        /// Indicates whether schema will be written in DDL
        /// </summary>
        /// <value><c>true</c> if schema is written; otherwise, <c>false</c>.</value>
        public bool IncludeSchema { get; set; }
        /// <summary>
        /// Escape the names (default true)
        /// </summary>
        public bool EscapeNames { get; set; }

        /// <summary>
        /// If available, use batching after each statement (in SqlServer, GO)
        /// </summary>
        public bool UseGranularBatching { get; set; }
    }
}