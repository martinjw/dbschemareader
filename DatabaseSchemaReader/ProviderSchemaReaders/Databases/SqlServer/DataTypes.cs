using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    class DataTypes
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>
            {
                new DataType("smallint", "System.Int16")
                {
                    ProviderDbType = 16,
                    CreateFormat = "smallint",
                },
                new DataType("int", "System.Int32")
                {
                    ProviderDbType = 8,
                    CreateFormat = "int",
                },
                new DataType("real", "System.Single")
                {
                    ProviderDbType = 13,
                    CreateFormat = "real",
                },
                new DataType("float", "System.Double")
                {
                    ProviderDbType = 6,
                    CreateFormat = "float({0})",
                },
                new DataType("money", "System.Decimal")
                {
                    ProviderDbType = 9,
                    CreateFormat = "money",
                },
                new DataType("smallmoney", "System.Decimal")
                {
                    ProviderDbType = 17,
                    CreateFormat = "smallmoney",
                },
                new DataType("bit", "System.Boolean")
                {
                    ProviderDbType = 2,
                    CreateFormat = "bit",
                },
                new DataType("tinyint", "System.Byte")
                {
                    ProviderDbType = 20,
                    CreateFormat = "tinyint",
                },
                new DataType("bigint", "System.Int64")
                {
                    ProviderDbType = 0,
                    CreateFormat = "bigint",
                },
                new DataType("timestamp", "System.Byte[]")
                {
                    ProviderDbType = 19,
                    CreateFormat = "timestamp",
                    LiteralPrefix = "0x",
                },
                new DataType("binary", "System.Byte[]")
                {
                    ProviderDbType = 1,
                    CreateFormat = "binary({0})",
                    LiteralPrefix = "0x",
                },
                new DataType("image", "System.Byte[]")
                {
                    ProviderDbType = 7,
                    CreateFormat = "image",
                    LiteralPrefix = "0x",
                },
                new DataType("text", "System.String")
                {
                    ProviderDbType = 18,
                    CreateFormat = "text",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("ntext", "System.String")
                {
                    ProviderDbType = 11,
                    CreateFormat = "ntext",
                    LiteralPrefix = "N'",
                    LiteralSuffix = "'",
                },
                new DataType("decimal", "System.Decimal")
                {
                    ProviderDbType = 5,
                    CreateFormat = "decimal({0}, {1})",
                },
                new DataType("numeric", "System.Decimal")
                {
                    ProviderDbType = 5,
                    CreateFormat = "numeric({0}, {1})",
                },
                new DataType("datetime", "System.DateTime")
                {
                    ProviderDbType = 4,
                    CreateFormat = "datetime",
                    LiteralPrefix = "{ts '",
                    LiteralSuffix = "'}",
                },
                new DataType("smalldatetime", "System.DateTime")
                {
                    ProviderDbType = 15,
                    CreateFormat = "smalldatetime",
                    LiteralPrefix = "{ts '",
                    LiteralSuffix = "'}",
                },
                new DataType("sql_variant", "System.Object")
                {
                    ProviderDbType = 23,
                    CreateFormat = "sql_variant",
                },
                new DataType("xml", "System.String")
                {
                    ProviderDbType = 25,
                    CreateFormat = "xml",
                },
                new DataType("varchar", "System.String")
                {
                    ProviderDbType = 22,
                    CreateFormat = "varchar({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("char", "System.String")
                {
                    ProviderDbType = 3,
                    CreateFormat = "char({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("nchar", "System.String")
                {
                    ProviderDbType = 10,
                    CreateFormat = "nchar({0})",
                    LiteralPrefix = "N'",
                    LiteralSuffix = "'",
                },
                new DataType("nvarchar", "System.String")
                {
                    ProviderDbType = 12,
                    CreateFormat = "nvarchar({0})",
                    LiteralPrefix = "N'",
                    LiteralSuffix = "'",
                },
                new DataType("varbinary", "System.Byte[]")
                {
                    ProviderDbType = 21,
                    CreateFormat = "varbinary({0})",
                    LiteralPrefix = "0x",
                },
                new DataType("uniqueidentifier", "System.Guid")
                {
                    ProviderDbType = 14,
                    CreateFormat = "uniqueidentifier",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("date", "System.DateTime")
                {
                    ProviderDbType = 31,
                    CreateFormat = "date",
                    LiteralPrefix = "{ts '",
                    LiteralSuffix = "'}",
                },
                new DataType("time", "System.TimeSpan")
                {
                    ProviderDbType = 32,
                    CreateFormat = "time({0})",
                    LiteralPrefix = "{ts '",
                    LiteralSuffix = "'}",
                },
                new DataType("datetime2", "System.DateTime")
                {
                    ProviderDbType = 33,
                    CreateFormat = "datetime2({0})",
                    LiteralPrefix = "{ts '",
                    LiteralSuffix = "'}",
                },
                new DataType("datetimeoffset", "System.DateTimeOffset")
                {
                    ProviderDbType = 34,
                    CreateFormat = "datetimeoffset({0})",
                    LiteralPrefix = "{ts '",
                    LiteralSuffix = "'}",
                }
            };
            return dts;
        }
    }
}
