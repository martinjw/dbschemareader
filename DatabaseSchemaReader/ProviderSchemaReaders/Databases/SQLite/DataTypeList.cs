using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>
            {
                new DataType("smallint", "System.Int16")
                {
                    ProviderDbType = 10,
                    CreateFormat = "smallint",
                },
                new DataType("int", "System.Int32")
                {
                    ProviderDbType = 11,
                    CreateFormat = "int",
                },
                new DataType("real", "System.Double")
                {
                    ProviderDbType = 8,
                    CreateFormat = "real",
                },
                new DataType("single", "System.Single")
                {
                    ProviderDbType = 15,
                    CreateFormat = "single",
                },
                new DataType("float", "System.Double")
                {
                    ProviderDbType = 8,
                    CreateFormat = "float",
                },
                new DataType("double", "System.Double")
                {
                    ProviderDbType = 8,
                    CreateFormat = "double",
                },
                new DataType("money", "System.Decimal")
                {
                    ProviderDbType = 7,
                    CreateFormat = "money",
                },
                new DataType("currency", "System.Decimal")
                {
                    ProviderDbType = 7,
                    CreateFormat = "currency",
                },
                new DataType("decimal", "System.Decimal")
                {
                    ProviderDbType = 7,
                    CreateFormat = "decimal",
                },
                new DataType("numeric", "System.Decimal")
                {
                    ProviderDbType = 7,
                    CreateFormat = "numeric",
                },
                new DataType("bit", "System.Boolean")
                {
                    ProviderDbType = 3,
                    CreateFormat = "bit",
                },
                new DataType("yesno", "System.Boolean")
                {
                    ProviderDbType = 3,
                    CreateFormat = "yesno",
                },
                new DataType("logical", "System.Boolean")
                {
                    ProviderDbType = 3,
                    CreateFormat = "logical",
                },
                new DataType("bool", "System.Boolean")
                {
                    ProviderDbType = 3,
                    CreateFormat = "bool",
                },
                new DataType("boolean", "System.Boolean")
                {
                    ProviderDbType = 3,
                    CreateFormat = "boolean",
                },
                new DataType("tinyint", "System.Byte")
                {
                    ProviderDbType = 2,
                    CreateFormat = "tinyint",
                },
                new DataType("integer", "System.Int64")
                {
                    ProviderDbType = 12,
                    CreateFormat = "integer",
                },
                new DataType("counter", "System.Int64")
                {
                    ProviderDbType = 12,
                    CreateFormat = "counter",
                },
                new DataType("autoincrement", "System.Int64")
                {
                    ProviderDbType = 12,
                    CreateFormat = "autoincrement",
                },
                new DataType("identity", "System.Int64")
                {
                    ProviderDbType = 12,
                    CreateFormat = "identity",
                },
                new DataType("long", "System.Int64")
                {
                    ProviderDbType = 12,
                    CreateFormat = "long",
                },
                new DataType("bigint", "System.Int64")
                {
                    ProviderDbType = 12,
                    CreateFormat = "bigint",
                },
                new DataType("binary", "System.Byte[]")
                {
                    ProviderDbType = 1,
                    CreateFormat = "binary",
                    LiteralPrefix = "X'",
                    LiteralSuffix = "'",
                },
                new DataType("varbinary", "System.Byte[]")
                {
                    ProviderDbType = 1,
                    CreateFormat = "varbinary",
                    LiteralPrefix = "X'",
                    LiteralSuffix = "'",
                },
                new DataType("blob", "System.Byte[]")
                {
                    ProviderDbType = 1,
                    CreateFormat = "blob",
                    LiteralPrefix = "X'",
                    LiteralSuffix = "'",
                },
                new DataType("image", "System.Byte[]")
                {
                    ProviderDbType = 1,
                    CreateFormat = "image",
                    LiteralPrefix = "X'",
                    LiteralSuffix = "'",
                },
                new DataType("general", "System.Byte[]")
                {
                    ProviderDbType = 1,
                    CreateFormat = "general",
                    LiteralPrefix = "X'",
                    LiteralSuffix = "'",
                },
                new DataType("oleobject", "System.Byte[]")
                {
                    ProviderDbType = 1,
                    CreateFormat = "oleobject",
                    LiteralPrefix = "X'",
                    LiteralSuffix = "'",
                },
                new DataType("varchar", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "varchar({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("nvarchar", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "nvarchar({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("memo", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "memo({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("longtext", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "longtext({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("note", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "note({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("text", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "text({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("ntext", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "ntext({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("string", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "string({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("char", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "char({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("nchar", "System.String")
                {
                    ProviderDbType = 16,
                    CreateFormat = "char({0})",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("datetime", "System.DateTime")
                {
                    ProviderDbType = 6,
                    CreateFormat = "datetime",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("smalldate", "System.DateTime")
                {
                    ProviderDbType = 6,
                    CreateFormat = "smalldate",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("timestamp", "System.DateTime")
                {
                    ProviderDbType = 6,
                    CreateFormat = "timestamp",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("date", "System.DateTime")
                {
                    ProviderDbType = 6,
                    CreateFormat = "date",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("time", "System.DateTime")
                {
                    ProviderDbType = 6,
                    CreateFormat = "time",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("uniqueidentifier", "System.Guid")
                {
                    ProviderDbType = 4,
                    CreateFormat = "uniqueidentifier",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                },
                new DataType("guid", "System.Guid")
                {
                    ProviderDbType = 4,
                    CreateFormat = "guid",
                    LiteralPrefix = "'",
                    LiteralSuffix = "'",
                }
            };
            return dts;
        }
    }
}
