using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServer
{
    class DataTypes
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>();
            dts.Add(new DataType("smallint", "System.Int16")
            {
                ProviderDbType = 16,
                CreateFormat = "smallint",
            });
            dts.Add(new DataType("int", "System.Int32")
            {
                ProviderDbType = 8,
                CreateFormat = "int",
            });
            dts.Add(new DataType("real", "System.Single")
            {
                ProviderDbType = 13,
                CreateFormat = "real",
            });
            dts.Add(new DataType("float", "System.Double")
            {
                ProviderDbType = 6,
                CreateFormat = "float({0})",
            });
            dts.Add(new DataType("money", "System.Decimal")
            {
                ProviderDbType = 9,
                CreateFormat = "money",
            });
            dts.Add(new DataType("smallmoney", "System.Decimal")
            {
                ProviderDbType = 17,
                CreateFormat = "smallmoney",
            });
            dts.Add(new DataType("bit", "System.Boolean")
            {
                ProviderDbType = 2,
                CreateFormat = "bit",
            });
            dts.Add(new DataType("tinyint", "System.Byte")
            {
                ProviderDbType = 20,
                CreateFormat = "tinyint",
            });
            dts.Add(new DataType("bigint", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "bigint",
            });
            dts.Add(new DataType("timestamp", "System.Byte[]")
            {
                ProviderDbType = 19,
                CreateFormat = "timestamp",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("binary", "System.Byte[]")
            {
                ProviderDbType = 1,
                CreateFormat = "binary({0})",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("image", "System.Byte[]")
            {
                ProviderDbType = 7,
                CreateFormat = "image",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("text", "System.String")
            {
                ProviderDbType = 18,
                CreateFormat = "text",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("ntext", "System.String")
            {
                ProviderDbType = 11,
                CreateFormat = "ntext",
                LiteralPrefix = "N'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("decimal", "System.Decimal")
            {
                ProviderDbType = 5,
                CreateFormat = "decimal({0}, {1})",
            });
            dts.Add(new DataType("numeric", "System.Decimal")
            {
                ProviderDbType = 5,
                CreateFormat = "numeric({0}, {1})",
            });
            dts.Add(new DataType("datetime", "System.DateTime")
            {
                ProviderDbType = 4,
                CreateFormat = "datetime",
                LiteralPrefix = "{ts '",
                LiteralSuffix = "'}",
            });
            dts.Add(new DataType("smalldatetime", "System.DateTime")
            {
                ProviderDbType = 15,
                CreateFormat = "smalldatetime",
                LiteralPrefix = "{ts '",
                LiteralSuffix = "'}",
            });
            dts.Add(new DataType("sql_variant", "System.Object")
            {
                ProviderDbType = 23,
                CreateFormat = "sql_variant",
            });
            dts.Add(new DataType("xml", "System.String")
            {
                ProviderDbType = 25,
                CreateFormat = "xml",
            });
            dts.Add(new DataType("varchar", "System.String")
            {
                ProviderDbType = 22,
                CreateFormat = "varchar({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("char", "System.String")
            {
                ProviderDbType = 3,
                CreateFormat = "char({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("nchar", "System.String")
            {
                ProviderDbType = 10,
                CreateFormat = "nchar({0})",
                LiteralPrefix = "N'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("nvarchar", "System.String")
            {
                ProviderDbType = 12,
                CreateFormat = "nvarchar({0})",
                LiteralPrefix = "N'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("varbinary", "System.Byte[]")
            {
                ProviderDbType = 21,
                CreateFormat = "varbinary({0})",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("uniqueidentifier", "System.Guid")
            {
                ProviderDbType = 14,
                CreateFormat = "uniqueidentifier",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("date", "System.DateTime")
            {
                ProviderDbType = 31,
                CreateFormat = "date",
                LiteralPrefix = "{ts '",
                LiteralSuffix = "'}",
            });
            dts.Add(new DataType("time", "System.TimeSpan")
            {
                ProviderDbType = 32,
                CreateFormat = "time({0})",
                LiteralPrefix = "{ts '",
                LiteralSuffix = "'}",
            });
            dts.Add(new DataType("datetime2", "System.DateTime")
            {
                ProviderDbType = 33,
                CreateFormat = "datetime2({0})",
                LiteralPrefix = "{ts '",
                LiteralSuffix = "'}",
            });
            dts.Add(new DataType("datetimeoffset", "System.DateTimeOffset")
            {
                ProviderDbType = 34,
                CreateFormat = "datetimeoffset({0})",
                LiteralPrefix = "{ts '",
                LiteralSuffix = "'}",
            });
            return dts;
        }
    }
}
