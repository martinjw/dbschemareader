using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.SQLite
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>();
            dts.Add(new DataType("smallint", "System.Int16")
            {
                ProviderDbType = 10,
                CreateFormat = "smallint",
            });
            dts.Add(new DataType("int", "System.Int32")
            {
                ProviderDbType = 11,
                CreateFormat = "int",
            });
            dts.Add(new DataType("real", "System.Double")
            {
                ProviderDbType = 8,
                CreateFormat = "real",
            });
            dts.Add(new DataType("single", "System.Single")
            {
                ProviderDbType = 15,
                CreateFormat = "single",
            });
            dts.Add(new DataType("float", "System.Double")
            {
                ProviderDbType = 8,
                CreateFormat = "float",
            });
            dts.Add(new DataType("double", "System.Double")
            {
                ProviderDbType = 8,
                CreateFormat = "double",
            });
            dts.Add(new DataType("money", "System.Decimal")
            {
                ProviderDbType = 7,
                CreateFormat = "money",
            });
            dts.Add(new DataType("currency", "System.Decimal")
            {
                ProviderDbType = 7,
                CreateFormat = "currency",
            });
            dts.Add(new DataType("decimal", "System.Decimal")
            {
                ProviderDbType = 7,
                CreateFormat = "decimal",
            });
            dts.Add(new DataType("numeric", "System.Decimal")
            {
                ProviderDbType = 7,
                CreateFormat = "numeric",
            });
            dts.Add(new DataType("bit", "System.Boolean")
            {
                ProviderDbType = 3,
                CreateFormat = "bit",
            });
            dts.Add(new DataType("yesno", "System.Boolean")
            {
                ProviderDbType = 3,
                CreateFormat = "yesno",
            });
            dts.Add(new DataType("logical", "System.Boolean")
            {
                ProviderDbType = 3,
                CreateFormat = "logical",
            });
            dts.Add(new DataType("bool", "System.Boolean")
            {
                ProviderDbType = 3,
                CreateFormat = "bool",
            });
            dts.Add(new DataType("boolean", "System.Boolean")
            {
                ProviderDbType = 3,
                CreateFormat = "boolean",
            });
            dts.Add(new DataType("tinyint", "System.Byte")
            {
                ProviderDbType = 2,
                CreateFormat = "tinyint",
            });
            dts.Add(new DataType("integer", "System.Int64")
            {
                ProviderDbType = 12,
                CreateFormat = "integer",
            });
            dts.Add(new DataType("counter", "System.Int64")
            {
                ProviderDbType = 12,
                CreateFormat = "counter",
            });
            dts.Add(new DataType("autoincrement", "System.Int64")
            {
                ProviderDbType = 12,
                CreateFormat = "autoincrement",
            });
            dts.Add(new DataType("identity", "System.Int64")
            {
                ProviderDbType = 12,
                CreateFormat = "identity",
            });
            dts.Add(new DataType("long", "System.Int64")
            {
                ProviderDbType = 12,
                CreateFormat = "long",
            });
            dts.Add(new DataType("bigint", "System.Int64")
            {
                ProviderDbType = 12,
                CreateFormat = "bigint",
            });
            dts.Add(new DataType("binary", "System.Byte[]")
            {
                ProviderDbType = 1,
                CreateFormat = "binary",
                LiteralPrefix = "X'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("varbinary", "System.Byte[]")
            {
                ProviderDbType = 1,
                CreateFormat = "varbinary",
                LiteralPrefix = "X'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("blob", "System.Byte[]")
            {
                ProviderDbType = 1,
                CreateFormat = "blob",
                LiteralPrefix = "X'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("image", "System.Byte[]")
            {
                ProviderDbType = 1,
                CreateFormat = "image",
                LiteralPrefix = "X'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("general", "System.Byte[]")
            {
                ProviderDbType = 1,
                CreateFormat = "general",
                LiteralPrefix = "X'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("oleobject", "System.Byte[]")
            {
                ProviderDbType = 1,
                CreateFormat = "oleobject",
                LiteralPrefix = "X'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("varchar", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "varchar({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("nvarchar", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "nvarchar({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("memo", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "memo({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("longtext", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "longtext({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("note", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "note({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("text", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "text({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("ntext", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "ntext({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("string", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "string({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("char", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "char({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("nchar", "System.String")
            {
                ProviderDbType = 16,
                CreateFormat = "char({0})",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("datetime", "System.DateTime")
            {
                ProviderDbType = 6,
                CreateFormat = "datetime",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("smalldate", "System.DateTime")
            {
                ProviderDbType = 6,
                CreateFormat = "smalldate",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("timestamp", "System.DateTime")
            {
                ProviderDbType = 6,
                CreateFormat = "timestamp",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("date", "System.DateTime")
            {
                ProviderDbType = 6,
                CreateFormat = "date",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("time", "System.DateTime")
            {
                ProviderDbType = 6,
                CreateFormat = "time",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("uniqueidentifier", "System.Guid")
            {
                ProviderDbType = 4,
                CreateFormat = "uniqueidentifier",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            dts.Add(new DataType("guid", "System.Guid")
            {
                ProviderDbType = 4,
                CreateFormat = "guid",
                LiteralPrefix = "'",
                LiteralSuffix = "'",
            });
            return dts;
        }
    }
}
