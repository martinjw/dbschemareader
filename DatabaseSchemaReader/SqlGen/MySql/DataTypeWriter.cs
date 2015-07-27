using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class DataTypeWriter : IDataTypeWriter
    {
        /// <summary>
        /// Gets the MySql datatype definition as string
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public string WriteDataType(DatabaseColumn column)
        {
            if (column == null) return string.Empty;
            if (string.IsNullOrEmpty(column.DbDataType)) return string.Empty;
            //we don't do column.DbDataTypeStandard() as native types will have length/precision-scale
            //and also ints will have the UNSIGNED marker
            //These types will fall through unchanged.
            var dataType = column.DbDataType.ToUpperInvariant();
            //int providerType = -1;
            //if (column.DataType != null)
            //    providerType = column.DataType.ProviderDbType;

            var precision = column.Precision;
            var scale = column.Scale;
            var length = column.Length;

            //oracle to MySql translation
            if (dataType == "NUMBER")
                dataType = DataTypeConverter.OracleNumberConversion(precision, scale);
            if (dataType.StartsWith("TIMESTAMP", StringComparison.OrdinalIgnoreCase) &&
                DataTypeConverter.IsSqlServerTimestamp(dataType, column))
            {
                dataType = "TINYBLOB"; //there's no equivalent really
            }

            if (dataType == "VARCHAR2" || dataType == "NVARCHAR" || dataType == "NVARCHAR2")
            {
                dataType = ConvertString(length);
            }
            else if (dataType == "CLOB" || dataType == "NTEXT")
            {
                dataType = "LONGTEXT";
            }
            else if (dataType == "NCHAR")
            {
                dataType = "CHAR";
            }
            else if (dataType == "DATETIME2" || dataType == "TIME")
            {
                dataType = "DATETIME";
            }
            else if (dataType == "MONEY")
            {
                dataType = "DECIMAL";
                precision = 19;
                scale = 4;
            }
            else if (dataType == "BIT")
            {
                dataType = "TINYINT";
            }

            else if (dataType == "IMAGE" || dataType == "VARBINARY")
            {
                dataType = ConvertBlob(length);
            }
            else if (dataType == "UNIQUEIDENTIFIER")
            {
                dataType = "VARCHAR";
                length = 64;
            }
            else if (dataType == "XML" || dataType == "XMLTYPE")
            {
                dataType = "TEXT";
            }
            //write out MySql datatype definition
            if (dataType == "VARCHAR" ||
                dataType == "CHAR" ||
                dataType == "BINARY" ||
                dataType == "VARBINARY")
            {

                dataType = dataType + " (" + length + ")";
            }

            if (dataType == "NUMERIC" ||
                dataType == "DECIMAL")
            {
                var writeScale = ((scale != null) && (scale > 0) ? "," + scale : "");
                dataType = dataType + " (" + precision + writeScale + ")";
            }

            return dataType;
        }

        private static string ConvertBlob(int? length)
        {
            if (length < 65536)
            {
                return "BLOB";
            }
            if (length >= 65536 && length < 16777216)
            {
                return "MEDIUMBLOB";
            }
            return "LONGBLOB";
        }

        private static string ConvertString(int? length)
        {
            var dataType = "VARCHAR";
            if (length == -1) //MAX
            {
                dataType = "LONGTEXT";
            }
            else if (length > 255 && length < 65536)
            {
                dataType = "TEXT";
            }
            else if (length >= 65536 && length < 16777216)
            {
                dataType = "MEDIUMTEXT";
            }
            else if (length >= 16777216)
            {
                dataType = "LONGTEXT";
            }
            return dataType;
        }
    }
}
