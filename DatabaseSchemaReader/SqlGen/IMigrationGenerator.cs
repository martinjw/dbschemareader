using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Performs simple database schema migrations
    /// </summary>
    public interface IMigrationGenerator
    {
        /// <summary>
        /// Writes the table. If any constraints are attached, they are written too (don't write them individually)
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <returns></returns>
        string CreateTable(DatabaseTable databaseTable);
        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="databaseColumn">The database column.</param>
        /// <returns></returns>
        string AddColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn);
        /// <summary>
        /// Alters the column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="originalColumn">The original column.</param>
        /// <returns></returns>
        string AlterColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, DatabaseColumn originalColumn);
        /// <summary>
        /// Drops the column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="databaseColumn">The database column.</param>
        /// <returns></returns>
        string DropColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn);

        /// <summary>
        /// Drops the table.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <returns></returns>
        string DropTable(DatabaseTable databaseTable);

        /// <summary>
        /// Adds the constraint.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="constraint">The constraint.</param>
        /// <returns></returns>
        string AddConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint);

        /// <summary>
        /// Drops the constraint.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="constraint">The constraint.</param>
        /// <returns></returns>
        string DropConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint);
    }
}