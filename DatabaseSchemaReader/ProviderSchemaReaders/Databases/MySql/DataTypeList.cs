﻿using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.MySql
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>
            {
                new DataType("BIT", "System.UInt64")
                {
                    ProviderDbType = 16,
                    CreateFormat = "BIT",
                },
                new DataType("BLOB", "System.Byte[]")
                {
                    ProviderDbType = 252,
                    CreateFormat = "",
                    LiteralPrefix = "0x",
                },
                new DataType("TINYBLOB", "System.Byte[]")
                {
                    ProviderDbType = 249,
                    CreateFormat = "",
                    LiteralPrefix = "0x",
                },
                new DataType("MEDIUMBLOB", "System.Byte[]")
                {
                    ProviderDbType = 250,
                    CreateFormat = "",
                    LiteralPrefix = "0x",
                },
                new DataType("LONGBLOB", "System.Byte[]")
                {
                    ProviderDbType = 251,
                    CreateFormat = "",
                    LiteralPrefix = "0x",
                },
                new DataType("BINARY", "System.Byte[]")
                {
                    ProviderDbType = 600,
                    CreateFormat = "binary({0})",
                    LiteralPrefix = "0x",
                },
                new DataType("VARBINARY", "System.Byte[]")
                {
                    ProviderDbType = 601,
                    CreateFormat = "varbinary({0})",
                    LiteralPrefix = "0x",
                },
                new DataType("DATE", "System.DateTime")
                {
                    ProviderDbType = 10,
                    CreateFormat = "DATE",
                },
                new DataType("DATETIME", "System.DateTime")
                {
                    ProviderDbType = 12,
                    CreateFormat = "DATETIME",
                },
                new DataType("TIMESTAMP", "System.DateTime")
                {
                    ProviderDbType = 7,
                    CreateFormat = "TIMESTAMP",
                },
                new DataType("TIME", "System.TimeSpan")
                {
                    ProviderDbType = 11,
                    CreateFormat = "TIME",
                },
                new DataType("CHAR", "System.String")
                {
                    ProviderDbType = 254,
                    CreateFormat = "CHAR({0})",
                },
                new DataType("NCHAR", "System.String")
                {
                    ProviderDbType = 254,
                    CreateFormat = "NCHAR({0})",
                },
                new DataType("VARCHAR", "System.String")
                {
                    ProviderDbType = 253,
                    CreateFormat = "VARCHAR({0})",
                },
                new DataType("NVARCHAR", "System.String")
                {
                    ProviderDbType = 253,
                    CreateFormat = "NVARCHAR({0})",
                },
                new DataType("SET", "System.String")
                {
                    ProviderDbType = 248,
                    CreateFormat = "SET",
                },
                new DataType("ENUM", "System.String")
                {
                    ProviderDbType = 247,
                    CreateFormat = "ENUM",
                },
                new DataType("TINYTEXT", "System.String")
                {
                    ProviderDbType = 749,
                    CreateFormat = "TINYTEXT",
                },
                new DataType("TEXT", "System.String")
                {
                    ProviderDbType = 752,
                    CreateFormat = "TEXT",
                },
                new DataType("MEDIUMTEXT", "System.String")
                {
                    ProviderDbType = 750,
                    CreateFormat = "MEDIUMTEXT",
                },
                new DataType("LONGTEXT", "System.String")
                {
                    ProviderDbType = 751,
                    CreateFormat = "LONGTEXT",
                },
                new DataType("DOUBLE", "System.Double")
                {
                    ProviderDbType = 5,
                    CreateFormat = "DOUBLE",
                },
                new DataType("FLOAT", "System.Single")
                {
                    ProviderDbType = 4,
                    CreateFormat = "FLOAT",
                },
                new DataType("TINYINT", "System.SByte")
                {
                    ProviderDbType = 1,
                    CreateFormat = "TINYINT",
                },
                new DataType("SMALLINT", "System.Int16")
                {
                    ProviderDbType = 2,
                    CreateFormat = "SMALLINT",
                },
                new DataType("INT", "System.Int32")
                {
                    ProviderDbType = 3,
                    CreateFormat = "INT",
                },
                new DataType("YEAR", "System.Int32")
                {
                    ProviderDbType = 13,
                    CreateFormat = "YEAR",
                },
                new DataType("MEDIUMINT", "System.Int32")
                {
                    ProviderDbType = 9,
                    CreateFormat = "MEDIUMINT",
                },
                new DataType("BIGINT", "System.Int64")
                {
                    ProviderDbType = 8,
                    CreateFormat = "BIGINT",
                },
                new DataType("DECIMAL", "System.Decimal")
                {
                    ProviderDbType = 246,
                    CreateFormat = "DECIMAL({0},{1})",
                },
                new DataType("TINY INT", "System.Byte")
                {
                    ProviderDbType = 501,
                    CreateFormat = "TINYINT UNSIGNED",
                },
                new DataType("SMALLINT", "System.UInt16")
                {
                    ProviderDbType = 502,
                    CreateFormat = "SMALLINT UNSIGNED",
                },
                new DataType("MEDIUMINT", "System.UInt32")
                {
                    ProviderDbType = 509,
                    CreateFormat = "MEDIUMINT UNSIGNED",
                },
                new DataType("INT", "System.UInt32")
                {
                    ProviderDbType = 503,
                    CreateFormat = "INT UNSIGNED",
                },
                new DataType("BIGINT", "System.UInt64")
                {
                    ProviderDbType = 508,
                    CreateFormat = "BIGINT UNSIGNED",
                }
            };
            return dts;
        }
    }
}
