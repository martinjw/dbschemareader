using System;
using System.Data;
using System.Globalization;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases
{
    static class DataRecordExtensions
    {
        public static string GetString(this IDataRecord record, string fieldName)
        {
            var value = record[fieldName];
            if (value == DBNull.Value) return null;
            return value.ToString();
        }

        public static int? GetNullableInt(this IDataRecord record, string fieldName)
        {
            var value = record[fieldName];
            try
            {
                return (value != DBNull.Value) ? System.Convert.ToInt32(value, CultureInfo.CurrentCulture) : (int?)null;
            }
            catch (OverflowException)
            {
                //this occurs for blobs and clobs using the OleDb provider
                return -1;
            }
        }

        public static int GetInt(this IDataRecord record, string fieldName)
        {
            return GetNullableInt(record, fieldName).GetValueOrDefault();
        }

        public static long? GetNullableLong(this IDataRecord record, string fieldName)
        {
            var value = record[fieldName];
            try
            {
                return (value != DBNull.Value) ? System.Convert.ToInt64(value, CultureInfo.CurrentCulture) : (long?)null;
            }
            catch (OverflowException)
            {
                //this occurs for blobs and clobs using the OleDb provider
                return -1;
            }
        }

        public static bool GetBoolean(this IDataRecord record, string fieldName)
        {
            var value = record[fieldName];
            if (value is bool) //SqlLite has a true boolean
            {
                return (bool)value;
            }
            var s = value.ToString();
            if (s == "0") return false;
            if (s == "1") return true;
            if (s == "-1") return true;
            //could be Y, YES, N, NO, true, false.
            if (s.StartsWith("Y", StringComparison.OrdinalIgnoreCase) || s.StartsWith("T", StringComparison.OrdinalIgnoreCase)) //Y or YES
                return true;
            if (s.StartsWith("N", StringComparison.OrdinalIgnoreCase) || s.StartsWith("F", StringComparison.OrdinalIgnoreCase)) //N or NO
                return false;
            return false;
        }
    }
}
