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
            int providerType = -1;
            if (column.DataType != null)
                providerType = column.DataType.ProviderDbType;

            var precision = column.Precision;
            var scale = column.Scale;
            var length = column.Length;

            //oracle to MySql translation

            if (dataType == "VARCHAR2" || dataType == "NVARCHAR")
            {
                dataType = "VARCHAR";
            }

            if (dataType == "DATETIME2" ||
                dataType == "TIME")
            {
                dataType = "DATETIME";
            }
            //if (dataType == "NUMERIC")
            //{
            //    dataType = "DECIMAL";
            //}
            if (dataType == "BIT")
            {
                dataType = "TINYINT";
            }

            //write out SqlServer datatype definition
            if (dataType == "VARCHAR" ||
                dataType == "CHAR" ||
                dataType == "BINARY" ||
                dataType == "VARBINARY")
            {
                dataType = dataType + " (" +
                    (length != -1 ? length.ToString() : "MAX")
                    + ")";
            }

            if (dataType == "NUMERIC" ||
                dataType == "DECIMAL")
            {
                var writeScale = ((scale != null) && (scale > 0) ? "," + scale.ToString() : "");
                dataType = dataType + " (" + precision + writeScale + ")";
            }

            return dataType;
        }
    }
}
