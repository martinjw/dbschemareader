using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SqlServerCe
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>();
            dts.Add(new DataType("smallint", "System.Int16")
            {
                ProviderDbType = 2,
                CreateFormat = "smallint",
            });
            dts.Add(new DataType("int", "System.Int32")
            {
                ProviderDbType = 3,
                CreateFormat = "int",
            });
            dts.Add(new DataType("real", "System.Single")
            {
                ProviderDbType = 4,
                CreateFormat = "real",
            });
            dts.Add(new DataType("float", "System.Double")
            {
                ProviderDbType = 5,
                CreateFormat = "float",
            });
            dts.Add(new DataType("money", "System.Decimal")
            {
                ProviderDbType = 6,
                CreateFormat = "money",
                LiteralPrefix = "$",
            });
            dts.Add(new DataType("bit", "System.Boolean")
            {
                ProviderDbType = 11,
                CreateFormat = "bit",
            });
            dts.Add(new DataType("tinyint", "System.Byte")
            {
                ProviderDbType = 17,
                CreateFormat = "tinyint",
            });
            dts.Add(new DataType("bigint", "System.Int64")
            {
                ProviderDbType = 20,
                CreateFormat = "bigint",
            });
            dts.Add(new DataType("uniqueidentifier", "System.Guid")
            {
                ProviderDbType = 72,
                CreateFormat = "uniqueidentifier",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("varbinary", "System.Byte[]")
            {
                ProviderDbType = 128,
                CreateFormat = "varbinary({0})",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("binary", "System.Byte[]")
            {
                ProviderDbType = 128,
                CreateFormat = "binary({0})",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("image", "System.Byte[]")
            {
                ProviderDbType = 128,
                CreateFormat = "image",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("nvarchar", "System.String")
            {
                ProviderDbType = 130,
                CreateFormat = "nvarchar({0})",
                LiteralPrefix = "N'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("nchar", "System.String")
            {
                ProviderDbType = 130,
                CreateFormat = "nchar({0})",
                LiteralPrefix = "N'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("ntext", "System.String")
            {
                ProviderDbType = 130,
                CreateFormat = "ntext",
                LiteralPrefix = "N'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("numeric", "System.Decimal")
            {
                ProviderDbType = 131,
                CreateFormat = "numeric({0}, {1})",
            });
            dts.Add(new DataType("datetime", "System.DateTime")
            {
                ProviderDbType = 135,
                CreateFormat = "datetime",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("rowversion", "System.Byte[]")
            {
                ProviderDbType = 128,
                CreateFormat = "timestamp",
                LiteralPrefix = "0x",
            });
            return dts;
        }
    }
}
