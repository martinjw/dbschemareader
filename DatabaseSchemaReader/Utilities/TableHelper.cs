using DatabaseSchemaReader.DataSchema;
using System.Linq;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    /// Static Helper class for Database Tables
    /// </summary>
    public static class TableHelper
    {
        /// <summary>
        /// Clones and returns a copy of this DatabaseTable
        /// </summary>
        /// <param name="table">The table being cloned</param>
        /// <param name="name">An optional new name to give the table</param>
        /// <returns>A DatabaseTable copy instance</returns>
        public static DatabaseTable Clone(this DatabaseTable table, string name = null)
        {
            var clone = new DatabaseTable
            {
                Name = string.IsNullOrEmpty(name) ? table.Name : name,
                PrimaryKey = table.PrimaryKey,
                Description = table.Description,
                SchemaOwner = table.SchemaOwner,
            };
            clone.Columns.AddRange(table.Columns);
            clone.Triggers.AddRange(table.Triggers);
            clone.Indexes.AddRange(table.Indexes);
            clone.ForeignKeys.AddRange(table.ForeignKeys);
            clone.ForeignKeyChildren.AddRange(table.ForeignKeyChildren);
            clone.UniqueKeys.AddRange(table.UniqueKeys);
            clone.CheckConstraints.AddRange(table.CheckConstraints);
            clone.DefaultConstraints.AddRange(table.DefaultConstraints);
            return clone;
        }

        /// <summary>
        /// Generates an SQL select all statement for this DatabaseTable
        /// </summary>
        /// <param name="table">The table to generate query for</param>
        /// <param name="sqlType">Optionally specify the sql type of the table DB</param>
        /// <returns>A string with the table select statement</returns>
        public static string SelectAll(this DatabaseTable table, SqlType sqlType)
        {
            return new SqlWriter(table, sqlType).SelectAllSql();
        }

        /// <summary>
        /// Gets an array of this DatabaseTable's columns
        /// </summary>
        /// <param name="table">The table to get columns of</param>
        /// <returns>A string array containing the table's columns</returns>
        public static string[] GetColumnList(this DatabaseTable table)
        {
            return table.Columns.Select(s => s.Name).ToArray();
        }

        /// <summary>
        /// Gets a delimiter separated/formatted list of the current DatabaseTable's columns
        /// </summary>
        /// <param name="table">The table to get formatted list of columns from</param>
        /// <param name="sqlType">The SqlType</param>
        /// <returns>A formatted string containing the table's columns</returns>
        public static string GetFormattedColumnList(this DatabaseTable table, SqlType sqlType)
        {
            return new SqlWriter(table, sqlType).FormattedColumns(table.GetColumnList());
        }
    }
}