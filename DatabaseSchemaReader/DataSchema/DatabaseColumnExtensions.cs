using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Helper methods for columns
    /// </summary>
    public static class DatabaseColumnExtensions
    {
        /// <summary>
        /// Determines whether this column is a timestamp.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public static bool IsTimestamp(this DatabaseColumn column)
        {
            if (column.DataType != null)
            {
                //if it's a timestamp, you can't insert it
                if (column.DataType.ProviderDbType == 19 //System.Data.SqlDbType.Timestamp
                    //double check as could be Oracle type with same provider code
                    && column.DataType.GetNetType() == typeof(byte[]))
                {
                    return true;
                }
            }
            return false;
        }

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
            if (String.IsNullOrEmpty(column.DbDataType)) return null;
            var dataType = column.DbDataType.ToUpperInvariant();
            var brace = dataType.IndexOf("(", StringComparison.OrdinalIgnoreCase);
            if (brace != -1) //timestamp(6)
            {
                dataType = Regex.Replace(dataType, "\\([^\\)]*\\)", string.Empty);
                //leave on trailing text
                //dataType = dataType.Substring(0, brace);
            }
            //also clean off MySql's unsigned indicator
            var space = dataType.IndexOf(" unsigned", StringComparison.OrdinalIgnoreCase);
            if (space > 1)
                dataType = dataType.Substring(0, space);
            return dataType;
        }

        /// <summary>
        /// Data type definition (suitable for DDL).
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The full datatype specification (including length, precision/scale as applicable)</returns>
        /// <remarks>
        /// Uses column.DbDataType and column.DataType.
        /// When writing full DDL, use the SqlGen DataTypeWriters.
        /// </remarks>
        public static string DataTypeDefinition(this DatabaseColumn column)
        {
            if (column == null) return null;
            var sb = new StringBuilder();
            var dbDataType = column.DbDataType ?? String.Empty;
            var brace = dbDataType.IndexOf("(", StringComparison.OrdinalIgnoreCase);
            if (brace != -1)
            {
                sb.Append(dbDataType);
            }
            else
            {
                var space = dbDataType.IndexOf(" ", StringComparison.OrdinalIgnoreCase);
                var unsigned = false;
                if (space > 1)
                {
                    unsigned = dbDataType.IndexOf("unsigned", StringComparison.OrdinalIgnoreCase) != -1;
                    dbDataType = dbDataType.Substring(0, space);
                }
                sb.Append(dbDataType);
                var dataType = column.DataType;
                if (dataType != null)
                {
                    if (dataType.IsString && !dataType.IsStringClob && column.Length != 0)
                    {
                        sb.Append("(");
                        var length = column.Length.GetValueOrDefault();
                        sb.Append(length != -1 ? length.ToString(CultureInfo.InvariantCulture) : "MAX");
                        sb.Append(")");
                    }
                    else if (dataType.IsNumeric && !dataType.IsInt)
                    {
                        sb.Append("(");
                        sb.Append(column.Precision);
                        sb.Append(",");
                        sb.Append(column.Scale);
                        sb.Append(")");
                    }
                    if (unsigned)
                    {
                        sb.Append(" UNSIGNED");
                    }
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns the .NET type of the column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns>The .NET type of the column</returns>
        /// <remarks>
        /// For numeric Db data types uses column.Precision and column.Scale to determine the correct .NET data type.
        /// </remarks>
        public static Type NetDataType(this DatabaseColumn column)
        {
            if (column == null) return null;
            if (column.DataType == null) return null;

            if (!column.DataType.IsNumeric || column.DataType.IsInt) return column.DataType.GetNetType();
            var precision = column.Precision.GetValueOrDefault();
            var scale = column.Scale.GetValueOrDefault();
            return NetTypeForIntegers(column, scale, precision);
        }


        private static Type NetTypeForIntegers(DatabaseColumn column, int scale, int precision)
        {
            if (scale != 0 || precision >= 19) return column.DataType.GetNetType();

            //could be a short, int or long...
            //VARCHAR2(10) is common for Oracle integers, but it can overflow an int
            //int.MaxValue is 2147483647 so +1 is allowable in the database
            if (precision > 10) //up to long.MaxValue
            {
                return typeof(long);
            }
            if (precision > 4) //2147483647
            {
                return typeof(int);
            }
            if (precision > 1)
            {
                return typeof(short);
            }
            return column.DataType.GetNetType();
        }
    }
}
