using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.Firebird
{
    class DataTypeList
    {
        public IList<DataType> Execute()
        {
            var dts = new List<DataType>();
            dts.Add(new DataType("array", "System.Array")
            {
                ProviderDbType = 0,
            });
            dts.Add(new DataType("bigint", "System.Int64")
            {
                ProviderDbType = 1,
            });
            dts.Add(new DataType("blob", "System.Byte[]")
            {
                ProviderDbType = 2,
            });
            dts.Add(new DataType("char", "System.String")
            {
                ProviderDbType = 4,
            });
            dts.Add(new DataType("date", "System.DateTime")
            {
                ProviderDbType = 5,
            });
            dts.Add(new DataType("decimal", "System.Decimal")
            {
                ProviderDbType = 6,
            });
            dts.Add(new DataType("double precision", "System.Double")
            {
                ProviderDbType = 7,
            });
            dts.Add(new DataType("float", "System.Single")
            {
                ProviderDbType = 8,
            });
            dts.Add(new DataType("integer", "System.Int32")
            {
                ProviderDbType = 10,
            });
            dts.Add(new DataType("numeric", "System.Decimal")
            {
                ProviderDbType = 11,
            });
            dts.Add(new DataType("smallint", "System.Int16")
            {
                ProviderDbType = 12,
            });
            dts.Add(new DataType("blob sub_type 1", "System.String")
            {
                ProviderDbType = 13,
            });
            dts.Add(new DataType("time", "System.TimeSpan")
            {
                ProviderDbType = 14,
            });
            dts.Add(new DataType("timestamp", "System.DateTime")
            {
                ProviderDbType = 15,
            });
            dts.Add(new DataType("varchar", "System.String")
            {
                ProviderDbType = 16,
            });

            return dts;
        }
    }
}
