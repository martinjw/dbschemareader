using System;
using System.Data;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.PostgreSql
{
    class DataTypeWriter
    {
        public string DataType(DatabaseColumn column)
        {
            var dataType = column.DbDataType.ToUpperInvariant();

            dataType = OtherDatabaseTypesToPostgreSql(dataType, column);

            if ((dataType.StartsWith("TIMESTAMP", StringComparison.OrdinalIgnoreCase) || dataType == "TIME") &&
                column.DateTimePrecision > 0)
                dataType = dataType + " (" + column.DateTimePrecision + ")";

            //write out datatype definition
            if ((dataType == "VARCHAR" || dataType == "CHAR") && column.Length > 0)
            {
                dataType = dataType + " (" + column.Length + ")";
            }

            if (dataType == "NUMERIC" || dataType == "DECIMAL")
            {
                var scale = column.Scale;
                var precision = column.Precision;

                var writeScale = ((scale != null) && (scale > 0) ? "," + scale : "");
                if (precision > 0) //Postgresql can have no specified precision
                    dataType = dataType + " (" + precision + writeScale + ")";
            }

            return dataType;
        }

        private static string OtherDatabaseTypesToPostgreSql(string dataType, DatabaseColumn column)
        {
            int providerType = -1;
            if (column.DataType != null)
                providerType = column.DataType.ProviderDbType;

            //string types
            //character(n) (aka char(n)) character varying(n) aka varchar(n) and text

            if (dataType == "VARCHAR2") dataType = "VARCHAR";
            else if (dataType == "NVARCHAR2") dataType = "VARCHAR";
            else if (dataType == "NVARCHAR")
            {
                if (column.Length == -1) dataType = "TEXT";
                else dataType = "VARCHAR";
            }
            else if (dataType == "NCHAR") dataType = "CHAR";
            else if (dataType == "NTEXT") dataType = "TEXT";
            else if (dataType == "CLOB") dataType = "TEXT";
            else if (dataType == "NCLOB") dataType = "TEXT";
            else if (dataType == "BPCHAR") dataType = "CHAR"; //blank padded char is an internal postgresql name for CHAR

            //numeric types
            else if (dataType == "INT") dataType = "INTEGER";
            else if (dataType == "INT4") dataType = "INTEGER"; //this is a PostgreSql alias, we'll use standard SQL
            //else if (dataType == "SERIAL") dataType = "INTEGER"; //this is a PostgreSql alias, we'll use standard SQL
            //else if (dataType == "BIGSERIAL") dataType = "BIGINT"; //this is a PostgreSql alias, we'll use standard SQL
            else if (dataType == "INT8") dataType = "BIGINT"; //this is a PostgreSql alias, we'll use standard SQL
            else if (dataType == "INT2") dataType = "SMALLINT"; //this is a PostgreSql alias, we'll use standard SQL
            else if (dataType == "NUMBER")
                dataType = NumberConversion(column.Precision, column.Scale);

            //float and real
            else if (dataType == "FLOAT4") dataType = "REAL"; //this is a PostgreSql alias, we'll use standard SQL
            else if (dataType == "FLOAT") dataType = "DOUBLE PRECISION";

            //date times
            //SqlServer Timestamp is a binary
            else if (dataType.StartsWith("TIMESTAMP", StringComparison.OrdinalIgnoreCase) && providerType == (int)SqlDbType.Timestamp)
                dataType = "BYTEA"; //this is just a byte array- functionally you should redesign the table and perhaps use the system extension columns

            else if (dataType == "DATETIME") dataType = "TIMESTAMP";
            else if (dataType == "DATETIME2") dataType = "TIMESTAMP";

            //blobs become object ids
            else if (dataType == "BLOB") dataType = "OID";
            else if (dataType == "VARBINARY") dataType = "OID";


            //other types
            else if (dataType == "XMLTYPE") dataType = "XML";
            else if (dataType == "UNIQUEIDENTIFIER") dataType = "UUID";
            return dataType;
        }


        private static string NumberConversion(int? precision, int? scale)
        {
            //same as Oracle to SqlServer
            if (precision < 38 && scale == 0) return "INTEGER";
            if (precision == 1 && scale == 0) return "BIT";
            if (precision == 18 && scale == 0) return "DECIMAL";
            if (precision == 15 && scale == 4) return "MONEY";
            return "NUMERIC";
        }
    }
}
