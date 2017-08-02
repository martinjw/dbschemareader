using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServerCe
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>
            {
                new DataType("smallint", "System.Int16")
                {
                    ProviderDbType = 2,
                    CreateFormat = "smallint",
                },
                new DataType("int", "System.Int32")
                {
                    ProviderDbType = 3,
                    CreateFormat = "int",
                },
                new DataType("real", "System.Single")
                {
                    ProviderDbType = 4,
                    CreateFormat = "real",
                },
                new DataType("float", "System.Double")
                {
                    ProviderDbType = 5,
                    CreateFormat = "float",
                },
                new DataType("money", "System.Decimal")
                {
                    ProviderDbType = 6,
                    CreateFormat = "money",
                    LiteralPrefix = "$",
                },
                new DataType("bit", "System.Boolean")
                {
                    ProviderDbType = 11,
                    CreateFormat = "bit",
                },
                new DataType("tinyint", "System.Byte")
                {
                    ProviderDbType = 17,
                    CreateFormat = "tinyint",
                },
                new DataType("bigint", "System.Int64")
                {
                    ProviderDbType = 20,
                    CreateFormat = "bigint",
                },
                new DataType("uniqueidentifier", "System.Guid")
                {
                    ProviderDbType = 72,
                    CreateFormat = "uniqueidentifier",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("varbinary", "System.Byte[]")
                {
                    ProviderDbType = 128,
                    CreateFormat = "varbinary({0})",
                    LiteralPrefix = "0x",
                },
                new DataType("binary", "System.Byte[]")
                {
                    ProviderDbType = 128,
                    CreateFormat = "binary({0})",
                    LiteralPrefix = "0x",
                },
                new DataType("image", "System.Byte[]")
                {
                    ProviderDbType = 128,
                    CreateFormat = "image",
                    LiteralPrefix = "0x",
                },
                new DataType("nvarchar", "System.String")
                {
                    ProviderDbType = 130,
                    CreateFormat = "nvarchar({0})",
                    LiteralPrefix = "N'",
                    LiteralSuffix = "'",
                },
                new DataType("nchar", "System.String")
                {
                    ProviderDbType = 130,
                    CreateFormat = "nchar({0})",
                    LiteralPrefix = "N'",
                    LiteralSuffix = "'",
                },
                new DataType("ntext", "System.String")
                {
                    ProviderDbType = 130,
                    CreateFormat = "ntext",
                    LiteralPrefix = "N'",
                    LiteralSuffix = "'",
                },
                new DataType("numeric", "System.Decimal")
                {
                    ProviderDbType = 131,
                    CreateFormat = "numeric({0}, {1})",
                },
                new DataType("datetime", "System.DateTime")
                {
                    ProviderDbType = 135,
                    CreateFormat = "datetime",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("rowversion", "System.Byte[]")
                {
                    ProviderDbType = 128,
                    CreateFormat = "timestamp",
                    LiteralPrefix = "0x",
                }
            };
            return dts;
        }
    }
}
