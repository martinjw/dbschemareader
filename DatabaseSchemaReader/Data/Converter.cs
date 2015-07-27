using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Data
{
    /// <summary>
    /// Converts data to strings
    /// </summary>
    public class Converter
    {
        private readonly SqlType _sqlType;
        private readonly IDictionary<string, string> _dateTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="Converter"/> class.
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        /// <param name="dateTypes">The date types, required to distinguish DATE vs TIME</param>
        public Converter(SqlType sqlType, IDictionary<string, string> dateTypes)
        {
            _dateTypes = dateTypes;
            _sqlType = sqlType;
        }

        /// <summary>
        /// Converts the specified data into a string
        /// </summary>
        /// <param name="type">The CLR type of the data.</param>
        /// <param name="data">The data.</param>
        /// <param name="columnName">Name of the column (optional, required to work out dateTime types)</param>
        /// <returns></returns>
        public string Convert(Type type, object data, string columnName)
        {
            if (data == null || data == DBNull.Value)
            {
                return "NULL";
            }
            if (type == typeof(string))
            {
                //double up any single quotes
                data = data.ToString().Replace("'", "''");
                if (_sqlType == SqlType.SqlServer || _sqlType == SqlType.SqlServerCe)
                {
                    return "N'" + data + "'";
                }
                return "'" + data + "'";
            }
            if (type == typeof(Guid))
            {
                return "'" + data + "'";
            }
            if (type == typeof(byte[]))
            {
                return ConvertBytes(data);
            }
            if (type == typeof(DateTime))
            {
                var dbType = "DATETIME";
                if (!string.IsNullOrEmpty(columnName) && _dateTypes.ContainsKey(columnName))
                    dbType = _dateTypes[columnName];
                return new DateConverter(_sqlType).Convert((DateTime)data, dbType);
            }
            if (type == typeof(decimal))
            {
                //must have the invariant decimal point (. not ,), remove trailing zeroes
                return ((decimal)data).ToString("G29", CultureInfo.InvariantCulture);
            }
            //all the numeric types
            if (type == typeof(int) || type == typeof(short) || type == typeof(long) ||
                type == typeof(uint) || type == typeof(ushort) || type == typeof(ulong) ||
                type == typeof(sbyte) || type == typeof(byte) || type == typeof(float) || type == typeof(double))
            {
                //must have the invariant decimal point (. not ,)
                return System.Convert.ToString(data, CultureInfo.InvariantCulture);
            }
            if (type == typeof(bool) && _sqlType != SqlType.PostgreSql)
            {
                return (bool)data ? "1" : "0";
            }
            if (type == typeof(DateTimeOffset))
            {
                return "'" + ((DateTimeOffset)data).ToString("yyyy-MM-dd HH:mm:ss.fffffff zzz", CultureInfo.InvariantCulture) + "'";
            }
            if (type == typeof(TimeSpan))
            {
                var ts = (TimeSpan)data;
                return "'" + ts + "'";
            }
            return data.ToString();
        }



        private string ConvertBytes(object data)
        {
            //this is only practical for small blobs - SqlServer and Db2 seem to work, others it's better to use parameters
            var bytes = (byte[])data;
            var sb = new StringBuilder();
            if (_sqlType == SqlType.Db2)
            {
                sb.Append("x'");
            }
            else
            {
                sb.Append("0x");
            }
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2", CultureInfo.InvariantCulture));
            }
            if (_sqlType == SqlType.Db2)
                sb.Append("'");
            return sb.ToString();
        }
    }
}