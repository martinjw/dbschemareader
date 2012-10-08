using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.PostgreSql
{
    class DataTypeWriter : IDataTypeWriter
    {
        public string WriteDataType(DatabaseColumn column)
        {
            if (column == null) return string.Empty;
            if (string.IsNullOrEmpty(column.DbDataType)) return string.Empty;
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
            //string types
            //character(n) (aka char(n)) character varying(n) aka varchar(n) and text
            if (DataTypeConverter.IsFixedLengthString(dataType))
            {
                return "CHAR";
            }
            if (DataTypeConverter.IsLongString(dataType))
            {
                return "TEXT";
            }
            if (DataTypeConverter.IsVariableString(dataType))
            {
                if (column.Length == -1) return "TEXT";
                return "VARCHAR";
            }

                //numeric types
            if (dataType == "INT") return "INTEGER";
            if (dataType == "INT4") return "INTEGER"; //this is a PostgreSql alias, we'll use standard SQL
                //else if (dataType == "SERIAL") return "INTEGER"; //this is a PostgreSql alias, we'll use standard SQL
                //else if (dataType == "BIGSERIAL") return "BIGINT"; //this is a PostgreSql alias, we'll use standard SQL
            if (dataType == "INT8") return "BIGINT"; //this is a PostgreSql alias, we'll use standard SQL
            if (dataType == "INT2") return "SMALLINT"; //this is a PostgreSql alias, we'll use standard SQL
            if (dataType == "TINYINT") return "SMALLINT"; //this is a MsSql alias, we'll use standard SQL
            if (dataType == "NUMBER")
                return DataTypeConverter.OracleNumberConversion(column.Precision, column.Scale);

                //float and real
            if (dataType == "FLOAT4") return "REAL"; //this is a PostgreSql alias, we'll use standard SQL
            if (dataType == "FLOAT") return "DOUBLE PRECISION";

                //date times
                //SqlServer Timestamp is a binary
            if (DataTypeConverter.IsSqlServerTimestamp(dataType, column))
                return "BYTEA"; //this is just a byte array- functionally you should redesign the table and perhaps use the system extension columns

            if (DataTypeConverter.IsDateTime(dataType))
                return "TIMESTAMP";

                //bytes
            if (DataTypeConverter.IsBlob(dataType, column))
                return "OID";//blobs become object ids
            if (DataTypeConverter.IsBinary(dataType))
            {
                return "BYTEA";
            }

                //there is a native BIT(n) type in Postgresql, but in conversion we probably mean boolean.
            if (dataType == "BIT" && !column.Length.HasValue) return "BOOLEAN";

                //other types
            if (dataType == "XMLTYPE") return "XML";
            if (dataType == "UNIQUEIDENTIFIER") return "UUID";
            return dataType;
        }
    }
}
