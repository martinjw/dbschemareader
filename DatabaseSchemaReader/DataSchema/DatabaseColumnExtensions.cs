using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Internal helper methods for columns
    /// </summary>
    internal static class DatabaseColumnExtensions
    {
        /// <summary>
        /// Returns the DbDataType in a standard format (uppercased, any braces removed).
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        /// <remarks>
        /// MySQL native types will be in the form "tinyint(5) unsigned". 
        /// For compatibility, this exposes "TINYINT" which matches what other databases use.
        /// </remarks>
        internal static string DbDataTypeStandard(this DatabaseColumn column)
        {
            if (column == null) return null;
            if (string.IsNullOrEmpty(column.DbDataType)) return null;
            var dataType = column.DbDataType.ToUpperInvariant();
            var brace = dataType.IndexOf("(", StringComparison.OrdinalIgnoreCase);
            if (brace != -1) //timestamp(6)
                dataType = dataType.Substring(0, brace);
            return dataType;
        }
    }
}
