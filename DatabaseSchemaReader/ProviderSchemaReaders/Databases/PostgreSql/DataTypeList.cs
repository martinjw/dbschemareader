using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>();
            dts.Add(new DataType("bigint", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("bigserial", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("binary", "System.Byte[]")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("bit varying", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("bit", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("bool", "System.Boolean")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("boolean", "System.Boolean")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("bpchar", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("bytea", "System.Byte[]")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("char", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("character varying", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("character", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("date", "System.DateTime")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("dec", "System.Decimal")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("decimal", "System.Decimal")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("double precision", "System.Double")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("double", "System.Double")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("float", "System.Single")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("float4", "System.Single")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("float8", "System.Double")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("int", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("int2", "System.Int16")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("int4", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("int8", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("integer", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("interval", "System.TimeSpan")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("line", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("money", "System.Double")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("numeric", "System.Decimal")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("real", "System.Single")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("serial", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("serial4", "System.Int32")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("serial8", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("smallint", "System.Int16")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("text", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("time", "System.TimeSpan")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("timestamp", "System.DateTime")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("timestamptz", "System.DateTime")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("timetz", "System.TimeSpan")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("varbit", "System.Int64")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            dts.Add(new DataType("varchar", "System.String")
            {
                ProviderDbType = 0,
                CreateFormat = "",
            });
            return dts;
        }
    }
}
