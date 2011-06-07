using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Data
{
    class Converter
    {
        private readonly SqlType _sqlType;
        private readonly IDictionary<string, string> _dateTypes;

        public Converter(SqlType sqlType, IDictionary<string, string> dateTypes)
        {
            _dateTypes = dateTypes;
            _sqlType = sqlType;
        }

        public string Convert(string columnName, Type type, object data)
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
            if (type == typeof(byte[]))
            {
                return ConvertBytes(data);
            }
            if (type == typeof(DateTime))
            {
                var dbType = _dateTypes[columnName];
                return new DateConverter(_sqlType).Convert((DateTime)data, dbType);
            }

            //all the numeric types
            if (type == typeof(int) || type == typeof(short) || type == typeof(long) ||
                type == typeof(uint) || type == typeof(ushort) || type == typeof(ulong) ||
                type == typeof(sbyte) || type == typeof(byte) ||
                type == typeof(decimal) || type == typeof(float) || type == typeof(double))
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
            } else
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
