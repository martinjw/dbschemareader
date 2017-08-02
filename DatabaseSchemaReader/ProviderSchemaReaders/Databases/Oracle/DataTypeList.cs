using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {


            var dts = new List<DataType>
            {
                new DataType("BFILE", "System.Byte[]")
                {
                    ProviderDbType = 1,
                    CreateFormat = "BFILE",
                },
                new DataType("BLOB", "System.Byte[]")
                {
                    ProviderDbType = 2,
                    CreateFormat = "BLOB",
                },
                new DataType("CHAR", "System.String")
                {
                    ProviderDbType = 3,
                    CreateFormat = "CHAR({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("CLOB", "System.String")
                {
                    ProviderDbType = 4,
                    CreateFormat = "CLOB",
                },
                new DataType("DATE", "System.DateTime")
                {
                    ProviderDbType = 6,
                    CreateFormat = "DATE",
                    LiteralPrefix = "TO_DATE('",
                    LiteralSuffix = "','YYYY-MM-DD HH24:MI:SS')",
                },
                new DataType("FLOAT", "System.Decimal")
                {
                    ProviderDbType = 29,
                    CreateFormat = "FLOAT",
                },
                new DataType("INTERVAL DAY TO SECOND", "System.TimeSpan")
                {
                    ProviderDbType = 7,
                    CreateFormat = "INTERVAL DAY({0}) TO SECOND({1})",
                    LiteralPrefix = "TO_DSINTERVAL('",
                    LiteralSuffix = "')",
                },
                new DataType("INTERVAL YEAR TO MONTH", "System.Int32")
                {
                    ProviderDbType = 8,
                    CreateFormat = "INTERVAL YEAR({0}) TO MONTH",
                    LiteralPrefix = "TO_YMINTERVAL('",
                    LiteralSuffix = "')",
                },
                new DataType("LONG", "System.String")
                {
                    ProviderDbType = 10,
                    CreateFormat = "LONG",
                },
                new DataType("LONG RAW", "System.Byte[]")
                {
                    ProviderDbType = 9,
                    CreateFormat = "LONG RAW",
                },
                new DataType("NCHAR", "System.String")
                {
                    ProviderDbType = 11,
                    CreateFormat = "NCHAR({0})",
                    LiteralPrefix = "N'",
                    LiteralSuffix = "'",
                },
                new DataType("NCLOB", "System.String")
                {
                    ProviderDbType = 12,
                    CreateFormat = "NCLOB",
                },
                new DataType("NUMBER", "System.Decimal")
                {
                    ProviderDbType = 13,
                    CreateFormat = "NUMBER ({0},{1})",
                },
                new DataType("NVARCHAR2", "System.String")
                {
                    ProviderDbType = 14,
                    CreateFormat = "NVARCHAR2({0})",
                    LiteralPrefix = "N'",
                    LiteralSuffix = "'",
                },
                new DataType("RAW", "System.Byte[]")
                {
                    ProviderDbType = 15,
                    CreateFormat = "RAW({0})",
                    LiteralPrefix = "HEXTORAW('",
                    LiteralSuffix = "')",
                },
                new DataType("ROWID", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "ROWID",
                },
                new DataType("TIMESTAMP", "System.DateTime")
                {
                    ProviderDbType = 18,
                    CreateFormat = "TIMESTAMP({0})",
                    LiteralPrefix = "TO_TIMESTAMP('",
                    LiteralSuffix = "','YYYY-MM-DD HH24:MI:SS.FF')",
                },
                new DataType("TIMESTAMP WITH LOCAL TIME ZONE", "System.DateTime")
                {
                    ProviderDbType = 19,
                    CreateFormat = "TIMESTAMP({0} WITH LOCAL TIME ZONE)",
                    LiteralPrefix = "TO_TIMESTAMP_TZ('",
                    LiteralSuffix = "','YYYY-MM-DD HH24:MI:SS.FF')",
                },
                new DataType("TIMESTAMP WITH TIME ZONE", "System.DateTime")
                {
                    ProviderDbType = 20,
                    CreateFormat = "TIMESTAMP({0} WITH TIME ZONE)",
                    LiteralPrefix = "TO_TIMESTAMP_TZ('",
                    LiteralSuffix = "','YYYY-MM-DD HH24:MI:SS.FF TZH:TZM')",
                },
                new DataType("VARCHAR2", "System.String")
                {
                    ProviderDbType = 22,
                    CreateFormat = "VARCHAR2({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                }
            };

            return dts;
        }
    }
}
