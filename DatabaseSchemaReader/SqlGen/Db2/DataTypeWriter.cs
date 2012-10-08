using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Db2
{
    class DataTypeWriter : IDataTypeWriter
    {
        public string WriteDataType(DatabaseColumn column)
        {
            if (column == null) return string.Empty;
            if (string.IsNullOrEmpty(column.DbDataType)) return string.Empty;
            var dataType = column.DbDataType.ToUpperInvariant();

            dataType = OtherDatabaseTypesToDb2(dataType, column);

            if ((dataType.StartsWith("TIMESTAMP", StringComparison.OrdinalIgnoreCase) || dataType == "TIME") &&
                column.DateTimePrecision > 0)
                dataType = dataType + " (" + column.DateTimePrecision + ")";

            //write out datatype definition
            if (IsString(column, dataType) && column.Length > 0)
            {
                dataType = dataType + " (" + column.Length + ")";
            }

            if (dataType == "NUMERIC" || dataType == "DECIMAL")
            {
                var scale = column.Scale;
                var precision = column.Precision;

                var writeScale = ((scale != null) && (scale > 0) ? "," + scale : "");
                if (precision > 0)
                    dataType = dataType + " (" + precision + writeScale + ")";
            }

            return dataType;
        }

        public static bool IsString(DatabaseColumn column, string dataType)
        {
            //all aliases for CHAR and VARCHAR. there's also a LONG VARCHAR
            if (DataTypeConverter.IsVariableString(dataType) ||
                DataTypeConverter.IsFixedLengthString(dataType))
                return true;
            if (column.DataType == null) return false;
            return column.DataType.IsString;
        }

        private static string OtherDatabaseTypesToDb2(string dataType, DatabaseColumn column)
        {
            //string types
            if (DataTypeConverter.IsFixedLengthString(dataType))
            {
                return "CHAR";
            }
            if (DataTypeConverter.IsLongString(dataType))
            {
                return dataType == "CLOB" ? "CLOB" : "DBCLOB";
            }
            if (DataTypeConverter.IsVariableString(dataType))
            {
                if (column.Length == -1) return "DBCLOB";
                return "VARCHAR";
            }

            //numeric types
            if (dataType == "SMALLINT") return dataType;
            if (dataType == "BIGINT") return dataType;
            if (dataType == "INTEGER") return dataType;

            if (dataType == "INT") return "INTEGER";
            if (dataType == "NUM") return "NUMERIC"; //DB2 alias
            if (dataType == "DEC") return "DECIMAL"; //DB2 alias
            if (dataType == "MONEY") return "DECIMAL(19,4)";
            if (dataType == "BIT") return "SMALLINT"; //could be CHAR(1) but nicer with an integer
            if (dataType == "NUMBER")
                return DataTypeConverter.OracleNumberConversion(column.Precision, column.Scale);

            //date times
            //SqlServer Timestamp is a binary
            if (DataTypeConverter.IsSqlServerTimestamp(dataType, column))
                return "GRAPHIC";

            if (DataTypeConverter.IsDateTime(dataType))
                return "TIMESTAMP";

            //bytes
            if (DataTypeConverter.IsBlob(dataType, column))
                return "BLOB";
            if (DataTypeConverter.IsBinary(dataType))
            {
                if (dataType == "LONG VARGRAPHIC") return dataType;
                if (dataType == "GRAPHIC") return dataType;
                return "VARGRAPHIC";
            }

            //other types
            if (dataType == "XMLTYPE") return "XML";
            if (dataType == "UNIQUEIDENTIFIER") return "CHAR(16) FOR BIT DATA";
            return dataType;
        }
    }
}
