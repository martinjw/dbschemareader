using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {


            var dts = new List<DataType>();
 dts.Add(new DataType("BFILE", "System.Byte[]") {
    ProviderDbType = 1,
    CreateFormat = "BFILE",
});
dts.Add(new DataType("BLOB", "System.Byte[]") {
    ProviderDbType = 2,
    CreateFormat = "BLOB",
});
dts.Add(new DataType("CHAR", "System.String") {
    ProviderDbType = 3,
    CreateFormat = "CHAR({0})",
    LiteralPrefix = "'",
    LiteralSuffix = "'",
});
dts.Add(new DataType("CLOB", "System.String") {
    ProviderDbType = 4,
    CreateFormat = "CLOB",
});
dts.Add(new DataType("DATE", "System.DateTime") {
    ProviderDbType = 6,
    CreateFormat = "DATE",
    LiteralPrefix = "TO_DATE('",
    LiteralSuffix = "','YYYY-MM-DD HH24:MI:SS')",
});
dts.Add(new DataType("FLOAT", "System.Decimal") {
    ProviderDbType = 29,
    CreateFormat = "FLOAT",
});
dts.Add(new DataType("INTERVAL DAY TO SECOND", "System.TimeSpan") {
    ProviderDbType = 7,
    CreateFormat = "INTERVAL DAY({0}) TO SECOND({1})",
    LiteralPrefix = "TO_DSINTERVAL('",
    LiteralSuffix = "')",
});
dts.Add(new DataType("INTERVAL YEAR TO MONTH", "System.Int32") {
    ProviderDbType = 8,
    CreateFormat = "INTERVAL YEAR({0}) TO MONTH",
    LiteralPrefix = "TO_YMINTERVAL('",
    LiteralSuffix = "')",
});
dts.Add(new DataType("LONG", "System.String") {
    ProviderDbType = 10,
    CreateFormat = "LONG",
});
dts.Add(new DataType("LONG RAW", "System.Byte[]") {
    ProviderDbType = 9,
    CreateFormat = "LONG RAW",
});
dts.Add(new DataType("NCHAR", "System.String") {
    ProviderDbType = 11,
    CreateFormat = "NCHAR({0})",
    LiteralPrefix = "N'",
    LiteralSuffix = "'",
});
dts.Add(new DataType("NCLOB", "System.String") {
    ProviderDbType = 12,
    CreateFormat = "NCLOB",
});
dts.Add(new DataType("NUMBER", "System.Decimal") {
    ProviderDbType = 13,
    CreateFormat = "NUMBER ({0},{1})",
});
dts.Add(new DataType("NVARCHAR2", "System.String") {
    ProviderDbType = 14,
    CreateFormat = "NVARCHAR2({0})",
    LiteralPrefix = "N'",
    LiteralSuffix = "'",
});
dts.Add(new DataType("RAW", "System.Byte[]") {
    ProviderDbType = 15,
    CreateFormat = "RAW({0})",
    LiteralPrefix = "HEXTORAW('",
    LiteralSuffix = "')",
});
dts.Add(new DataType("ROWID", "System.String") {
    ProviderDbType = 16,
    CreateFormat = "ROWID",
});
dts.Add(new DataType("TIMESTAMP", "System.DateTime") {
    ProviderDbType = 18,
    CreateFormat = "TIMESTAMP({0})",
    LiteralPrefix = "TO_TIMESTAMP('",
    LiteralSuffix = "','YYYY-MM-DD HH24:MI:SS.FF')",
});
dts.Add(new DataType("TIMESTAMP WITH LOCAL TIME ZONE", "System.DateTime") {
    ProviderDbType = 19,
    CreateFormat = "TIMESTAMP({0} WITH LOCAL TIME ZONE)",
    LiteralPrefix = "TO_TIMESTAMP_TZ('",
    LiteralSuffix = "','YYYY-MM-DD HH24:MI:SS.FF')",
});
dts.Add(new DataType("TIMESTAMP WITH TIME ZONE", "System.DateTimeOffset") {
    ProviderDbType = 20,
    CreateFormat = "TIMESTAMP({0} WITH TIME ZONE)",
    LiteralPrefix = "TO_TIMESTAMP_TZ('",
    LiteralSuffix = "','YYYY-MM-DD HH24:MI:SS.FF TZH:TZM')",
});
dts.Add(new DataType("VARCHAR2", "System.String") {
    ProviderDbType = 22,
    CreateFormat = "VARCHAR2({0})",
    LiteralPrefix = "'",
    LiteralSuffix = "'",
});

            return dts;
        }
    }
}
