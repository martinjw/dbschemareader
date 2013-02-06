using System;
using System.Data;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    /// <summary>
    /// Returns a datatype string (will convert common Oracle types to SqlServer)
    /// </summary>
    /// <remarks>
    /// <see cref="DatabaseSchemaReader.SqlGen.SqlServerCe.DataTypeWriter"/> is derived from this for SqlServerCe
    /// </remarks>
    class DataTypeWriter : IDataTypeWriter
    {
        public DataTypeWriter()
            : this(null)
        {
        }
        public DataTypeWriter(SqlType? originSqlType)
        {
            _originSqlType = originSqlType;
        }

        private readonly SqlType? _originSqlType;


        public static string WriteDataType(string dataType)
        {
            //don't know provider
            return OracleToSqlServerConversion(dataType, -1, 0, 0);
        }

        /// <summary>
        /// Gets the SQLServer datatype definition as string
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public virtual string WriteDataType(DatabaseColumn column)
        {
            if (column == null) return string.Empty;
            if (string.IsNullOrEmpty(column.DbDataType)) return string.Empty;
            var dataType = column.DbDataType.ToUpperInvariant();
            int providerType = -1;
            if (column.DataType != null)
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

            //In SqlServer, the maximum length allowed for any data type is 8000.
            //Ergo, TEXTs and NTEXTs that are larger (int.MaxValue or int.MaxValue/2) must be SqlServer types
            if (dataType == "NTEXT" && length > 8000)
            {
                return "NTEXT";
            }
            if (dataType == "TEXT" && length > 8000)
            {
                return "TEXT";
            }
            if (_originSqlType == SqlType.SqlServer || _originSqlType == SqlType.SqlServerCe)
            {
                if (dataType == "BINARY")
                {
                    //should not be varbinary
                    return WriteDataTypeWithLength(dataType, length);
                }
            }
            else
            {
                dataType = ConvertOtherPlatformTypes(dataType, providerType, length, precision, scale);
            }

            if ((dataType == "DATETIME2" || dataType == "TIME") && column.DateTimePrecision.HasValue)
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
                dataType = WriteDataTypeWithLength(dataType, length);
            }

            if (dataType == "NUMERIC" ||
                dataType == "DECIMAL")
            {
                if (precision != null)
                {
                    var writeScale = ((scale != null) && (scale > 0) ? "," + scale : "");
                    dataType = dataType + " (" + precision + writeScale + ")";
                }
            }

            return dataType;
        }

        protected virtual string WriteDataTypeWithLength(string dataType, int? length)
        {
            if (length == 0) length = -1; //a zero length varchar doesn't make sense
            dataType = dataType + " (" +
                       (length != -1 ? length.ToString() : "MAX")
                       + ")";
            return dataType;
        }

        protected virtual string ConvertOtherPlatformTypes(string dataType, int providerType, int? length, int? precision, int? scale)
        {
            dataType = PostgreSqlToSqlServerConversion(dataType);
            dataType = AccessToSqlServerConversion(dataType);
            //a text with a defined length = probably a NvarChar
            if (dataType == "TEXT")
            {
                if (length == 0) return "NVARCHAR (MAX)"; //SqlServer TEXT will have length NULL
                if (length > 0) return "NVARCHAR";
            }
            return OracleToSqlServerConversion(dataType, providerType, precision, scale);
        }
        private static string PostgreSqlToSqlServerConversion(string dataType)
        {
            //PostgreSql specific types and the SqlServer equivalent
            //if (dataType == "VARCHAR") return "NVARCHAR";
            if (dataType == "CHARACTER VARYING") return "NVARCHAR";
            if (dataType == "CHARACTER") return "NCHAR";
            if (dataType == "BPCHAR") return "NCHAR";
            if (dataType == "INTEGER") return "INT";
            if (dataType == "INT4") return "INT";
            if (dataType == "SERIAL") return "INT";
            if (dataType == "BIGSERIAL") return "BIGINT";
            if (dataType == "INT8") return "BIGINT";
            if (dataType == "INT2") return "SMALLINT";
            if (dataType == "FLOAT4") return "REAL";
            if (dataType == "DOUBLE PRECISION") return "FLOAT";
            if (dataType == "BYTEA") return "VARBINARY";
            if (dataType == "UUID") return "UNIQUEIDENTIFIER";
            if (dataType == "OID") return "VARBINARY";
            return dataType;
        }

        private static string AccessToSqlServerConversion(string dataType)
        {
            if (dataType == "LONG") return "INT";
            if (dataType == "SINGLE") return "REAL";
            if (dataType == "BYTE") return "TINYINT";
            if (dataType == "SHORT") return "SMALLINT";
            if (dataType == "BOOLEAN") return "BIT";
            if (dataType == "CURRENCY") return "MONEY";
            if (dataType == "MEMO") return "NVARCHAR (MAX)";
            if (dataType == "BINARY") return "VARBINARY";
            if (dataType == "GUID") return "UNIQUEIDENTIFIER";
            return dataType;
        }

        private static string OracleToSqlServerConversion(string dataType, int providerType, int? precision, int? scale)
        {
            if (dataType == "VARCHAR2") return "VARCHAR";
            if (dataType == "NVARCHAR2") return "NVARCHAR";
            //DateTime in SQL Server range from 1753 A.D. to 9999 A.D., whereas dates in Oracle range from 4712 B.C. to 4712 A.D. For 2008, DateTime2 is 0001-9999, plus more accuracy.
            if (dataType == "DATE" && providerType != (int)SqlDbType.Date)
                return "DATETIME";
            //Oracle timestamp is a date with fractional sections. SqlServer timestamp is a binary type used for optimistic concurrency.
            if (dataType.StartsWith("TIMESTAMP", StringComparison.OrdinalIgnoreCase) && providerType != (int)SqlDbType.Timestamp)
                return "DATETIME";
            //Oracle numbers- use precise SqlServer versiom
            if (dataType == "NUMBER")
                return DataTypeConverter.OracleNumberConversion(precision, scale);
            //not an exact match
            if (dataType == "XMLTYPE") return "XML";
            return dataType;
        }

        /// <summary>
        /// If a table has a trigger, we assume it's an Oracle trigger/sequence which is translated to identity for the primary key
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public static bool LooksLikeOracleIdentityColumn(DatabaseTable table, DatabaseColumn column)
        {
            if (!column.IsPrimaryKey) return false;
            if (table.Triggers.Count == 0) return false;
            //is there a trigger body which looks like it's using a sequence?
            //if there's a sequence there, it's autogenerating a column - we assume the primary key!
            return table.Triggers.Any(t => t.TriggerBody
                                               .ToUpperInvariant()
                                               .Contains(".NEXTVAL "));
        }
    }
}
