using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>
            {
                new DataType("bigint", "System.Int64")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("bigserial", "System.Int64")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("binary", "System.Byte[]")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("bit varying", "System.Int64")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("bit", "System.Int64")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("bool", "System.Boolean")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("boolean", "System.Boolean")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("bpchar", "System.String")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("bytea", "System.Byte[]")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("char", "System.String")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("character varying", "System.String")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("character", "System.String")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("date", "System.DateTime")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("dec", "System.Decimal")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("decimal", "System.Decimal")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("double precision", "System.Double")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("double", "System.Double")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("float", "System.Single")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("float4", "System.Single")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("float8", "System.Double")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("int", "System.Int32")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("int2", "System.Int16")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("int4", "System.Int32")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("int8", "System.Int64")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("integer", "System.Int32")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("interval", "System.TimeSpan")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("line", "System.String")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("money", "System.Double")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("numeric", "System.Decimal")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("real", "System.Single")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("serial", "System.Int32")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("serial4", "System.Int32")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("serial8", "System.Int64")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("smallint", "System.Int16")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("text", "System.String")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("time", "System.TimeSpan")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("timestamp", "System.DateTime")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("timestamptz", "System.DateTime")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("timetz", "System.TimeSpan")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("varbit", "System.Int64")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                },
                new DataType("varchar", "System.String")
                {
                    ProviderDbType = 0,
                    CreateFormat = "",
                }
            };
            return dts;
        }
    }
}
