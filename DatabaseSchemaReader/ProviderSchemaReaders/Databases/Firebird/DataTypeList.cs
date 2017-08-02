using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>
            {
                new DataType("array", "System.Array")
                {
                    ProviderDbType = 0,
                },
                new DataType("bigint", "System.Int64")
                {
                    ProviderDbType = 1,
                },
                new DataType("blob", "System.Byte[]")
                {
                    ProviderDbType = 2,
                },
                new DataType("char", "System.String")
                {
                    ProviderDbType = 4,
                },
                new DataType("date", "System.DateTime")
                {
                    ProviderDbType = 5,
                },
                new DataType("decimal", "System.Decimal")
                {
                    ProviderDbType = 6,
                },
                new DataType("double precision", "System.Double")
                {
                    ProviderDbType = 7,
                },
                new DataType("float", "System.Single")
                {
                    ProviderDbType = 8,
                },
                new DataType("integer", "System.Int32")
                {
                    ProviderDbType = 10,
                },
                new DataType("numeric", "System.Decimal")
                {
                    ProviderDbType = 11,
                },
                new DataType("smallint", "System.Int16")
                {
                    ProviderDbType = 12,
                },
                new DataType("blob sub_type 1", "System.String")
                {
                    ProviderDbType = 13,
                },
                new DataType("time", "System.TimeSpan")
                {
                    ProviderDbType = 14,
                },
                new DataType("timestamp", "System.DateTime")
                {
                    ProviderDbType = 15,
                },
                new DataType("varchar", "System.String")
                {
                    ProviderDbType = 16,
                }
            };

            return dts;
        }
    }
}
