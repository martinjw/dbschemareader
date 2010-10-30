using System.Data;
using DatabaseSchemaReader.DataSchema;

namespace Library.Data.SqlGen.SqlServer
{
    /// <summary>
    /// Returns a datatype string (will convert common Oracle types to SqlServer)
    /// </summary>
    static class DataTypeWriter
    {
        public static string SqlServerDataType(string dataType)
        {
            //don't know provider
            return OracleToSqlServerConversion(dataType, -1, 0, 0);
        }
        
        /// <summary>
        /// Gets the SQLServer datatype definition as string
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public static string SqlServerDataType(this DatabaseColumn column)
        {
            var dataType = column.DbDataType.ToUpperInvariant();
            int providerType = -1;
            if(column.DataType != null) 
                providerType = column.DataType.ProviderDbType;

            var precision = column.Precision;
            var scale = column.Scale;
            var length = column.Length;

            //oracle to sql server translation
            if (dataType == "BLOB")
            {
                dataType = "VARBINARY";
                length = -1;
            }
            if (dataType == "CLOB")
            {
                dataType = "NVARCHAR";
                length = -1;
            }
            dataType = OracleToSqlServerConversion(dataType, providerType, precision, scale);

            if (dataType == "DATETIME2" || 
                dataType == "TIME")
            {
                dataType = dataType + "(" + column.DateTimePrecision + ")";
            }

            //write out SqlServer datatype definition
            if (dataType == "NVARCHAR" || 
                dataType == "VARCHAR" || 
                dataType == "CHAR" || 
                dataType == "NCHAR" || 
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
                var writeScale = ((scale != null) && (scale > 0) ? "," + scale : "");
                dataType = dataType + " (" + precision + writeScale + ")";
            }

            return dataType;
        }

        private static string OracleToSqlServerConversion(string dataType, int providerType, int? precision, int? scale)
        {
            if (dataType == "VARCHAR2") dataType = "NVARCHAR";
            if (dataType == "NVARCHAR2") dataType = "NVARCHAR";
            //DateTime in SQL Server range from 1753 A.D. to 9999 A.D., whereas dates in Oracle range from 4712 B.C. to 4712 A.D. For 2008, DateTime2 is 0001-9999, plus more accuracy.
            if (dataType == "DATE" && providerType != (int)SqlDbType.Date)
                dataType = "DATETIME";
            //Oracle timestamp is a date with fractional sections. SqlServer timestamp is a binary type used for optimistic concurrency.
            if (dataType.StartsWith("TIMESTAMP") && providerType != (int)SqlDbType.Timestamp)
                dataType = "DATETIME";
            //Oracle numbers- use precise SqlServer versiom
            if (dataType == "NUMBER" && precision < 38 && scale == 0) dataType = "INT";
            if (dataType == "NUMBER" && precision == 1 && scale == 0) dataType = "BIT";
            if (dataType == "NUMBER" && precision == 18 && scale == 0) dataType = "DECIMAL";
            if (dataType == "NUMBER" && precision == 15 && scale == 4) dataType = "MONEY";
            if (dataType == "NUMBER") dataType = "NUMERIC";
            return dataType;
        }
    }
}
