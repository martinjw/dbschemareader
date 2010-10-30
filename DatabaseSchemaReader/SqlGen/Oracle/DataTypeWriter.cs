using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Oracle
{
    /// <summary>
    /// Returns a datatype string (will convert common Oracle types to SqlServer)
    /// </summary>
    static class DataTypeWriter
    {
        public static string OracleDataType(string dataType)
        {
            //don't know provider
            return SqlServerToOracleConversion(dataType, -1, -1);
        }

        public static string OracleDataTypeForParameter(this DatabaseColumn column)
        {
            var dataType = column.DbDataType.ToUpperInvariant();
            var brace = dataType.IndexOf("(");
            if (brace != -1) //timestamp(6)
                dataType = dataType.Substring(0, brace);
            int providerType = GetProviderType(column);
            var length = column.Length;

            //oracle to sql server translation
            dataType = SqlServerToOracleConversion(dataType, providerType, length);

            return dataType;
        }

        /// <summary>
        /// Gets the Oracle datatype definition as string
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public static string OracleDataType(this DatabaseColumn column)
        {
            var dataType = column.DbDataType.ToUpperInvariant();
            int providerType = GetProviderType(column);

            var precision = column.Precision;
            var scale = column.Scale;
            var length = column.Length;

            //oracle to sql server translation
            dataType = SqlServerToOracleConversion(dataType, providerType, length);

            if (dataType == "NUMERIC" ||
                dataType == "DECIMAL")
            {
                var writeScale = ((scale != null) && (scale > 0) ? "," + scale : "");
                dataType = dataType + " (" + precision + writeScale + ")";
            }

            return dataType;
        }

        private static int GetProviderType(DatabaseColumn column)
        {
            int providerType = -1;
            if (column.DataType != null)
                providerType = column.DataType.ProviderDbType;
            return providerType;
        }

        private static string SqlServerToOracleConversion(string dataType, int providerType, int? length)
        {
            //oracle to sql server translation
            if (dataType == "VARBINARY") dataType = "BLOB";
            if (dataType == "IMAGE") dataType = "BLOB";
            if (dataType == "NVARCHAR" && length > 2000) dataType = "CLOB";
            if (dataType == "NTEXT") dataType = "CLOB";
            if (dataType == "TEXT") dataType = "CLOB";
            //you probably want Unicode.
            if (dataType == "VARCHAR") dataType = "NVARCHAR2";
            if (dataType == "NVARCHAR") dataType = "NVARCHAR2";
            //DateTime in SQL Server range from 1753 A.D. to 9999 A.D., whereas dates in Oracle range from 4712 B.C. to 4712 A.D. For 2008, DateTime2 is 0001-9999, plus more accuracy.
            if (dataType == "DATETIME") dataType = "DATE";
            //Oracle timestamp is a date with fractional sections. SqlServer timestamp is a binary type used for optimistic concurrency.
            if (dataType.StartsWith("TIMESTAMP") && providerType != 0x12 && providerType != 0x13 && providerType != 20)
            {
                dataType = "NUMBER";
            }
            if (dataType == "INT") dataType = "NUMBER";

            return dataType;
        }
    }
}
