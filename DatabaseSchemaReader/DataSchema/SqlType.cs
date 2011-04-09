
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
        SQLite
    }
}
