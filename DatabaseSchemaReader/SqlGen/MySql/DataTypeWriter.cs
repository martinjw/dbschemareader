using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    static class DataTypeWriter
    {
        /// <summary>
        /// Gets the MySql datatype definition as string
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public static string MySqlDataType(this DatabaseColumn column)
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

            if (dataType == "VARCHAR2" || dataType == "NVARCHAR")
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
            else if (dataType == "DATETIME2" ||
                dataType == "TIME")
            {
                dataType = "DATETIME";
            }
            else if (dataType == "MONEY")
            {
                dataType = "DECIMAL";
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
            //write out SqlServer datatype definition
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
