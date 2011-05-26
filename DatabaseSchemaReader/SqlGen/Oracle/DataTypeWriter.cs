using System;
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
            var brace = dataType.IndexOf("(", StringComparison.OrdinalIgnoreCase);
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

            dataType = PostgreSqlToSqlServerConversion(dataType);
            //oracle to sql server translation
            dataType = SqlServerToOracleConversion(dataType, providerType, length);

            if (dataType == "NUMERIC" || dataType == "DECIMAL")
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

        private static string PostgreSqlToSqlServerConversion(string dataType)
        {
            //PostgreSql specific types and the SqlServer equivalent
            if (dataType == "VARCHAR") return "NVARCHAR2";
            if (dataType == "CHARACTER VARYING") return "NVARCHAR2";
            if (dataType == "CHARACTER") return "NCHAR";
            if (dataType == "BPCHAR") return "NCHAR";
            if (dataType == "INTEGER") return "NUMBER(10)";
            if (dataType == "INT4") return "NUMBER(10)";
            if (dataType == "SERIAL") return "NUMBER(10)";
            if (dataType == "BIGSERIAL") return "BIGINT";
            if (dataType == "INT8") return "BIGINT";
            if (dataType == "INT2") return "SMALLINT";
            if (dataType == "FLOAT4") return "REAL";
            if (dataType == "BYTEA") return "BLOB";
            if (dataType == "UUID") return "RAW(16)";
            if (dataType == "OID") return "BLOB";
            if (dataType == "XML") return "XMLTYPE";
            return dataType;
        }

        private static string SqlServerToOracleConversion(string dataType, int providerType, int? length)
        {
            //oracle to sql server translation
            if (dataType == "VARBINARY") return "BLOB";
            if (dataType == "IMAGE") return "BLOB";
            if (dataType == "NVARCHAR" && length > 2000) return "CLOB";
            if (dataType == "NTEXT" || dataType == "TEXT") return "CLOB";
            //you probably want Unicode.
            if (dataType == "VARCHAR" || dataType == "NVARCHAR") return "NVARCHAR2";

            //DateTime in SQL Server range from 1753 A.D. to 9999 A.D., whereas dates in Oracle range from 4712 B.C. to 4712 A.D. For 2008, DateTime2 is 0001-9999, plus more accuracy.
            if (dataType == "DATETIME") return "DATE";
            //Oracle timestamp is a date with fractional sections. SqlServer timestamp is a binary type used for optimistic concurrency.
            if (dataType.StartsWith("TIMESTAMP", StringComparison.OrdinalIgnoreCase) && providerType != 0x12 && providerType != 0x13 && providerType != 20)
            {
                return "NUMBER";
            }
            if (dataType == "XML") return "XMLTYPE";

            return dataType;
        }

        public static string WriteDataType(DatabaseColumn column)
        {
            var sql = string.Empty;
            var dataType = column.DbDataType.ToUpperInvariant();
            var precision = column.Precision;
            var scale = column.Scale;
            var length = column.Length;

            if (dataType == "BOOLEAN")
            {
                dataType = "NUMBER";
                precision = 1;
                scale = 0;
            }
            //sql server to oracle translation
            dataType = SqlServerToOracleConversion(dataType, GetProviderType(column), length);

            if (dataType == "UNIQUEIDENTIFIER")
            {
                dataType = "RAW";
                length = 16;
            }
            if (dataType == "NUMERIC") dataType = "NUMBER";
            if (dataType == "INT")
            {
                dataType = "NUMBER";
                precision = 9;
                scale = 0;
            }
            if (dataType == "SMALLINT")
            {
                dataType = "NUMBER";
                precision = 5;
                scale = 0;
            }
            if (dataType == "BIT")
            {
                dataType = "NUMBER";
                precision = 1;
                scale = 0;
            }
            if (dataType == "DECIMAL")
            {
                dataType = "NUMBER";
                precision = 18;
                scale = 0;
            }
            if (dataType == "MONEY")
            {
                dataType = "NUMBER";
                precision = 15;
                scale = 4;
            }

            string defaultValue = FixDefaultValue(column);

            //write out Oracle datatype definition
            if (dataType == "NVARCHAR2")
            {
                //don't specify "CHAR" for NVARCHAR2
                sql = dataType + " (" + length + ")";
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT " + AddQuotedDefault(defaultValue);
            }
            if (dataType == "VARCHAR2")
            {
                //assume it's CHAR rather than bytes
                sql = dataType + " (" + length + " CHAR)";
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT " + AddQuotedDefault(defaultValue);
            }
            if (dataType == "CHAR" || dataType == "NCHAR")
            {
                sql = dataType + " (" + length + ")";
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT " + AddQuotedDefault(defaultValue);
            }
            if (dataType == "NUMBER")
            {
                if (!precision.HasValue)
                    sql = "NUMBER";
                else
                {
                    var writeScale = ((scale != null) && (scale > 0) ? "," + scale : "");
                    sql = "NUMBER (" + precision + writeScale + ")";
                }
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT " + defaultValue;
            }
            if (dataType == "REAL")
            {
                sql = "REAL";
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT " + defaultValue;
            }
            if (dataType == "RAW")
            {
                sql = "RAW(" + length + ")";
            }


            if (dataType == "DATE")
            {
                sql = "DATE";
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT DATE '" + defaultValue + "'";
            }

            if (dataType == "TIMESTAMP")
            {
                sql = "TIMESTAMP" + (precision.HasValue ? " (" + precision + ")" : " (6)");
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT TIMESTAMP '" + defaultValue + "'";
            }

            if (dataType == "CLOB")
            {
                sql = "CLOB ";
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT " + AddQuotedDefault(defaultValue);
            }

            if (dataType == "BLOB")
            {
                sql = "BLOB ";
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT " + AddQuotedDefault(defaultValue);
            }

            if (string.IsNullOrEmpty(sql))
            {
                sql = column.DbDataType;
                if (!string.IsNullOrEmpty(defaultValue))
                    sql += " DEFAULT " + AddQuotedDefault(defaultValue);
            }

            return sql + (!column.Nullable ? " NOT NULL" : string.Empty);
        }

        private static string FixDefaultValue(DatabaseColumn column)
        {
            //Guid defaults. 
            if (SqlTranslator.IsGuidGenerator(column.DefaultValue))
            {
                return "SYS_GUID()";
            }
            return SqlTranslator.Fix(column.DefaultValue);
        }

        private static string AddQuotedDefault(string defaultValue)
        {
            return "'" + defaultValue + "'";
        }
    }
}
