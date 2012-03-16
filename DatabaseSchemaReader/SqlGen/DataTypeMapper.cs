using System;
using System.Data;

namespace DatabaseSchemaReader.SqlGen
{
    abstract class DataTypeMapper
    {
        public abstract string Map(DbType dbType);

        public string Map<T>() where T : struct
        {
            //map common .Net types to the DbType, and then to the platform type
            var type = typeof (T);
            return Map(type);
        }

        public string Map(Type type)
        {
            if (type == typeof(string))
                return Map(DbType.String);
            if (type == typeof(char))
                return Map(DbType.AnsiStringFixedLength);

            if (type == typeof(decimal))
                return Map(DbType.Decimal);
            if (type == typeof(double))
                return Map(DbType.Double);
            if (type == typeof(float))
                return Map(DbType.Double);


            if (type == typeof(short))
                return Map(DbType.Int16);
            if (type == typeof(int))
                return Map(DbType.Int32);
            if (type == typeof(long))
                return Map(DbType.Int64);

            if (type == typeof(DateTime))
                return Map(DbType.DateTime);
            if (type == typeof(byte[]))
                return Map(DbType.Binary);
            if (type == typeof(Guid))
                return Map(DbType.Guid);

            if (type == typeof(bool))
                return Map(DbType.Boolean);

            return null;
        }
    }
}
