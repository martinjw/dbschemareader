using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    static class DataTypeConverter
    {
        public static bool IsVariableString(string dataType)
        {
            return (dataType == "VARCHAR2" ||
                    dataType == "NVARCHAR2" ||
                    dataType == "NVARCHAR" ||
                    dataType == "VARCHAR" ||
                    dataType == "LONG VARCHAR" ||
                    dataType == "CHARACTER VARYING" ||
                    dataType == "TEXT" ||
                    dataType == "NTEXT" ||
                    dataType == "CHAR VARYING");
        }
        public static bool IsFixedLengthString(string dataType)
        {
            return (dataType == "NCHAR" ||
                    dataType == "CHAR" ||
                    dataType == "BPCHAR" ||
                    dataType == "CHARACTER");
        }
        public static bool IsLongString(string dataType)
        {
            return (dataType == "TEXT" ||
                    dataType == "CLOB" ||
                    dataType == "DBCLOB" ||
                    dataType == "NTEXT");
        }
        public static bool IsBinary(string dataType)
        {
            return (dataType == "VARBINARY" ||
                    dataType == "RAW" ||
                    dataType == "LONG RAW" ||
                    dataType == "TINYBLOB" ||
                    dataType == "BYTEA" ||
                    dataType == "GRAPHIC" ||
                    dataType == "VARGRAPHIC" ||
                    dataType == "LONG VARGRAPHIC");
        }
        public static bool IsBlob(string dataType, DatabaseColumn column)
        {
            //special case SQLServer
            if (dataType == "VARBINARY" && column.Length == -1)
                return true; //VARBINARY(MAX)
            return (dataType == "BLOB" ||
                    dataType == "MEDIUMBLOB" ||
                    dataType == "LONGBLOB" ||
                    dataType == "IMAGE" ||
                    dataType == "OID");
        }
        public static bool HasPrecision(string dataType)
        {
            return (dataType == "DEC" ||
                    dataType == "DECIMAL" ||
                    dataType == "NUMBER" ||
                    dataType == "NUMERIC" ||
                    dataType == "FLOAT" ||
                    dataType == "DOUBLE" ||
                    dataType == "DOUBLE PRECISION");
        }
        public static bool IsDateTime(string dataType)
        {
            return (dataType == "DATETIME" ||
                    dataType == "DATETIME2" ||
                    dataType == "TIMESTAMP" ||
                    dataType == "SMALLDATETIME");
        }
        public static bool IsSqlServerTimestamp(string dataType, DatabaseColumn column)
        {
            if (!dataType.StartsWith("TIMESTAMP", StringComparison.OrdinalIgnoreCase))
                return false;
            int providerType = -1;
            if (column.DataType != null)
                providerType = column.DataType.ProviderDbType;
            return (providerType == (int)SqlDbType.Timestamp); //this is just a byte array
        }
        public static bool IsInteger(string dataType)
        {
            return (dataType == "INTEGER" ||
                    dataType == "INT" ||
                    dataType == "BIGINT" ||
                    dataType == "SMALLINT" ||
                    dataType == "TINYINT");
        }

        public static string OracleNumberConversion(int? precision, int? scale)
        {
            //this is an Oracle NUMBER (not NUMERIC) which we can convert down
            if (precision < 38 && scale == 0) return "INTEGER";
            if (precision == 1 && scale == 0) return "BIT";
            if (precision == 18 && scale == 0) return "DECIMAL";
            if (precision == 15 && scale == 4) return "MONEY";
            return "NUMERIC";
        }

        public static DatabaseColumn ParseDataType(string dataType)
        {
            var column = new DatabaseColumn();
            if (string.IsNullOrEmpty(dataType))
                return column;
            var brace = dataType.IndexOf('(');
            if (brace == -1)
            {
                //"INT"
                column.DbDataType = dataType;
                return column;
            }
            column.DbDataType = dataType.Substring(0, brace).Trim();

            brace = brace + 1;
            var endBrace = dataType.IndexOf(')', brace);
            var param = dataType.Substring(brace, endBrace - brace).Trim();
            if (param.IndexOf("MAX", StringComparison.OrdinalIgnoreCase) != -1)
            {
                //"VARCHAR(MAX)"
                column.Length = -1;
                return column;
            }
            var comma = param.IndexOf(',');

            if (comma == -1)
            {
                //"VARCHAR(20)"
                int length;
                if (int.TryParse(param, out length))
                {
                    var hasPrecision = HasPrecision(column.DbDataType.ToUpperInvariant());
                    if (hasPrecision)
                        column.Precision = length;
                    else
                        column.Length = length;
                }
                return column;
            }
            //"NUMBER(10,2)"
            var p1 = param.Substring(0, comma).Trim();
            var p2 = param.Substring(comma + 1).Trim();
            int precision, scale;
            if (int.TryParse(p1, out precision) && int.TryParse(p2, out scale))
            {
                column.Precision = precision;
                column.Scale = scale;
            }

            return column;
        }

        /// <summary>
        /// Adds a dataType object to a column.
        /// </summary>
        /// <param name="column">The column.</param>
        public static void AddDataType(DatabaseColumn column)
        {
            if (column == null) return;
            //there is no data type at all
            if (string.IsNullOrEmpty(column.DbDataType)) return;
            //a datatype already assigned
            if (column.DataType != null) return;
            //use upper case
            var dbType = column.DbDataType.ToUpper(CultureInfo.InvariantCulture);
            //nothing to convert
            if (string.IsNullOrEmpty(dbType)) return;
            var sqlType = SqlType.SqlServer;
            var dataTypeList = new List<DataType>();
            if (column.Table != null)
            {
                //look up the full schema if it exists
                var schema = column.Table.DatabaseSchema;
                if (schema != null)
                {
                    var provider = schema.Provider;
                    sqlType = Conversion.ProviderToSqlType.Convert(provider) ?? SqlType.SqlServer;
                    dataTypeList = schema.DataTypes;
                }
            }
            //does the schema data types contain this type? if so, assign it.
            var match = dataTypeList.FirstOrDefault(d => d.TypeName.Equals(dbType, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                column.DataType = match;
                return;
            }
            //we need to invent a dataType - we need a .Net typeName
            if (IsVariableString(dbType) || IsFixedLengthString(dbType))
            {
                var dataType = new DataType(dbType, typeof(string).FullName);
                column.DataType = dataType;
                dataTypeList.Add(dataType);
                return;
            }
            if (IsBinary(dbType) || IsBlob(dbType, column))
            {
                var dataType = new DataType(dbType, "System.Byte[]");
                column.DataType = dataType;
                dataTypeList.Add(dataType);
                return;
            }

            if (sqlType == SqlType.SqlServer &&
                dbType.StartsWith("TIMESTAMP(", StringComparison.OrdinalIgnoreCase))
            {
                var dataType = new DataType(dbType, "System.Byte[]");
                dataType.ProviderDbType = (int)SqlDbType.Timestamp;
                column.DataType = dataType;
                dataTypeList.Add(dataType);
                return;
            }
            if (IsDateTime(dbType))
            {
                var dataType = new DataType(dbType, typeof(DateTime).FullName);
                column.DataType = dataType;
                dataTypeList.Add(dataType);
                return;
            }
            if (IsInteger(dbType))
            {
                var netType = typeof(int).FullName;
                if (dbType.Equals("BIGINT")) netType = typeof(long).FullName;
                var dataType = new DataType(dbType, netType);
                column.DataType = dataType;
                dataTypeList.Add(dataType);
                return;
            }
            if (HasPrecision(dbType))
            {
                var dataType = new DataType(dbType, typeof(decimal).FullName);
                column.DataType = dataType;
                dataTypeList.Add(dataType);
            }
            //we've picked off the most common types here
        }

    }
}
