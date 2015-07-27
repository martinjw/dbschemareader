using System;
using System.Text.RegularExpressions;

namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Utilities to translate provider-specific SQL
    /// </summary>
    static class SqlTranslator
    {

        /// <summary>
        /// Sanitizes a SQL string (don't use GetDate(), remove parenthesis) 
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string Fix(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            value = EnsureCurrentTimestamp(value);
            //remove braces around numbers
            value = RemoveParenthesis(value);
            value = Regex.Replace(value, @"\((\d+)\)", "$1");
            return value;
        }

        /// <summary>
        /// SqlServer GetDate() function should be SQL-92 Current_Timestamp (also supported in SqlServer)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string EnsureCurrentTimestamp(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            //access
            var regex = new Regex(@"\bDate\(\)", RegexOptions.Compiled);
            if (regex.IsMatch(value))
            {
                value = regex.Replace(value, "current_timestamp");
            }

            //SQLServer function
            var getDate = value.IndexOf("getdate()", StringComparison.OrdinalIgnoreCase);
            if (getDate != -1)
            {
                value = value.Remove(getDate, 9).Insert(getDate, "current_timestamp");
            }
            //Oracle SYSDATE is actually current_timestamp(3) in Oracle (timestamp includes seconds) 
            var sysDate = value.IndexOf("sysdate", StringComparison.OrdinalIgnoreCase);
            if (sysDate != -1)
            {
                value = value.Remove(sysDate, 7).Insert(sysDate, "current_timestamp");
            }
            return value;
        }

        public static string RemoveParenthesis(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.StartsWith("((", StringComparison.OrdinalIgnoreCase) &&
                value.EndsWith("))", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2, value.Length - 4);
            }
            return value;
        }

        /// <summary>
        /// Determines whether the default value is a GUID generator
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        ///   <c>true</c> if is a GUID generator; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsGuidGenerator(string defaultValue)
        {
            return (
                //sqlserver
                "newid()".Equals(defaultValue, StringComparison.OrdinalIgnoreCase) ||
                "newsequentialid()".Equals(defaultValue, StringComparison.OrdinalIgnoreCase) ||
                //oracle
                "sys_guid()".Equals(defaultValue, StringComparison.OrdinalIgnoreCase) ||
                //mysql
                "uuid()".Equals(defaultValue, StringComparison.OrdinalIgnoreCase) ||
                //postgresql
                "uuid_generate_v1()".Equals(defaultValue, StringComparison.OrdinalIgnoreCase));
        }
    }
}
