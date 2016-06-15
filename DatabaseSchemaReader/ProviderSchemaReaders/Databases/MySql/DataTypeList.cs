using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>();
            dts.Add(new DataType("BIT", "System.UInt64")
            {
                ProviderDbType = 16,
                CreateFormat = "BIT",
            });
            dts.Add(new DataType("BLOB", "System.Byte[]")
            {
                ProviderDbType = 252,
                CreateFormat = "",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("TINYBLOB", "System.Byte[]")
            {
                ProviderDbType = 249,
                CreateFormat = "",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("MEDIUMBLOB", "System.Byte[]")
            {
                ProviderDbType = 250,
                CreateFormat = "",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("LONGBLOB", "System.Byte[]")
            {
                ProviderDbType = 251,
                CreateFormat = "",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("BINARY", "System.Byte[]")
            {
                ProviderDbType = 600,
                CreateFormat = "binary({0})",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("VARBINARY", "System.Byte[]")
            {
                ProviderDbType = 601,
                CreateFormat = "varbinary({0})",
                LiteralPrefix = "0x",
            });
            dts.Add(new DataType("DATE", "System.DateTime")
            {
                ProviderDbType = 10,
                CreateFormat = "DATE",
            });
            dts.Add(new DataType("DATETIME", "System.DateTime")
            {
                ProviderDbType = 12,
                CreateFormat = "DATETIME",
            });
            dts.Add(new DataType("TIMESTAMP", "System.DateTime")
            {
                ProviderDbType = 7,
                CreateFormat = "TIMESTAMP",
            });
            dts.Add(new DataType("TIME", "System.TimeSpan")
            {
                ProviderDbType = 11,
                CreateFormat = "TIME",
            });
            dts.Add(new DataType("CHAR", "System.String")
            {
                ProviderDbType = 254,
                CreateFormat = "CHAR({0})",
            });
            dts.Add(new DataType("NCHAR", "System.String")
            {
                ProviderDbType = 254,
                CreateFormat = "NCHAR({0})",
            });
            dts.Add(new DataType("VARCHAR", "System.String")
            {
                ProviderDbType = 253,
                CreateFormat = "VARCHAR({0})",
            });
            dts.Add(new DataType("NVARCHAR", "System.String")
            {
                ProviderDbType = 253,
                CreateFormat = "NVARCHAR({0})",
            });
            dts.Add(new DataType("SET", "System.String")
            {
                ProviderDbType = 248,
                CreateFormat = "SET",
            });
            dts.Add(new DataType("ENUM", "System.String")
            {
                ProviderDbType = 247,
                CreateFormat = "ENUM",
            });
            dts.Add(new DataType("TINYTEXT", "System.String")
            {
                ProviderDbType = 749,
                CreateFormat = "TINYTEXT",
            });
            dts.Add(new DataType("TEXT", "System.String")
            {
                ProviderDbType = 752,
                CreateFormat = "TEXT",
            });
            dts.Add(new DataType("MEDIUMTEXT", "System.String")
            {
                ProviderDbType = 750,
                CreateFormat = "MEDIUMTEXT",
            });
            dts.Add(new DataType("LONGTEXT", "System.String")
            {
                ProviderDbType = 751,
                CreateFormat = "LONGTEXT",
            });
            dts.Add(new DataType("DOUBLE", "System.Double")
            {
                ProviderDbType = 5,
                CreateFormat = "DOUBLE",
            });
            dts.Add(new DataType("FLOAT", "System.Single")
            {
                ProviderDbType = 4,
                CreateFormat = "FLOAT",
            });
            dts.Add(new DataType("TINYINT", "System.SByte")
            {
                ProviderDbType = 1,
                CreateFormat = "TINYINT",
            });
            dts.Add(new DataType("SMALLINT", "System.Int16")
            {
                ProviderDbType = 2,
                CreateFormat = "SMALLINT",
            });
            dts.Add(new DataType("INT", "System.Int32")
            {
                ProviderDbType = 3,
                CreateFormat = "INT",
            });
            dts.Add(new DataType("YEAR", "System.Int32")
            {
                ProviderDbType = 13,
                CreateFormat = "YEAR",
            });
            dts.Add(new DataType("MEDIUMINT", "System.Int32")
            {
                ProviderDbType = 9,
                CreateFormat = "MEDIUMINT",
            });
            dts.Add(new DataType("BIGINT", "System.Int64")
            {
                ProviderDbType = 8,
                CreateFormat = "BIGINT",
            });
            dts.Add(new DataType("DECIMAL", "System.Decimal")
            {
                ProviderDbType = 246,
                CreateFormat = "DECIMAL({0},{1})",
            });
            dts.Add(new DataType("TINY INT", "System.Byte")
            {
                ProviderDbType = 501,
                CreateFormat = "TINYINT UNSIGNED",
            });
            dts.Add(new DataType("SMALLINT", "System.UInt16")
            {
                ProviderDbType = 502,
                CreateFormat = "SMALLINT UNSIGNED",
            });
            dts.Add(new DataType("MEDIUMINT", "System.UInt32")
            {
                ProviderDbType = 509,
                CreateFormat = "MEDIUMINT UNSIGNED",
            });
            dts.Add(new DataType("INT", "System.UInt32")
            {
                ProviderDbType = 503,
                CreateFormat = "INT UNSIGNED",
            });
            dts.Add(new DataType("BIGINT", "System.UInt64")
            {
                ProviderDbType = 508,
                CreateFormat = "BIGINT UNSIGNED",
            });
            return dts;
        }
    }
}
