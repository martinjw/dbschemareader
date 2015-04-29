
namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Database platform types supported for generating SQL.
    /// </summary>
    public enum SqlType
    {
        /// <summary>
        /// Microsoft SQL Server (2005, 2008, 2008 R2) including Express versions
        /// </summary>
        SqlServer,
        /// <summary>
        /// Oracle platforms (Oracle 9- 11, including XE)
        /// </summary>
        Oracle,
        /// <summary>
        /// MySQL (v5 onwards as we assume support for stored procedures)
        /// </summary>
        MySql,
        /// <summary>
        /// SQLite
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Lite")]
        SQLite,
        ///<summary>
        /// Microsoft SQL Server CE 4
        ///</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Ce")]
        SqlServerCe,
        /// <summary>
        /// PostgreSql
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Postgre")]
        PostgreSql,
        /// <summary>
        /// IBM DB2
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        Db2
    }
}
