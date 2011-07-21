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
                dataType = "VARCHAR";
                if (length == -1) //MAX
                {
                    dataType = "TEXT";
                }
            }
            else if (dataType == "CLOB")
            {
                dataType = "TEXT";
            }
            else if (dataType == "NTEXT")
            {
                dataType = "TEXT";
            }
            else if (dataType == "NCHAR")
            {
                dataType = "CHAR";
            }
            else if (dataType == "DATETIME2" ||
                dataType == "TIME")
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

            else if (dataType == "IMAGE")
            {
                dataType = "BLOB";
            }
            else if (dataType == "VARBINARY" && length == -1)
            {
                dataType = "BLOB";
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
    }
}
