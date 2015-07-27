using System.Collections.Generic;
using System.Data;

namespace DatabaseSchemaReader.SqlGen.SqLite
{
    class SqLiteDataTypeMapper : DataTypeMapper
    {
        private readonly IDictionary<DbType, string> _mapping = new Dictionary<DbType, string>();

        public SqLiteDataTypeMapper()
        {
            Init();
        }
        private void Init()
        {
            _mapping.Add(DbType.AnsiStringFixedLength, "TEXT");
            _mapping.Add(DbType.AnsiString, "TEXT");
            _mapping.Add(DbType.Binary, "BLOB");
            _mapping.Add(DbType.Boolean, "NUMERIC(1)");
            _mapping.Add(DbType.Byte, "NUMERIC(1)");
            _mapping.Add(DbType.Currency, "NUMERIC(15,4)");
            _mapping.Add(DbType.Date, "DATETIME");
            _mapping.Add(DbType.DateTime, "DATETIME");
            _mapping.Add(DbType.DateTime2, "DATETIME");
            _mapping.Add(DbType.DateTimeOffset, "DATETIME");
            _mapping.Add(DbType.Decimal, "NUMERIC");
            _mapping.Add(DbType.Double, "NUMERIC");
            _mapping.Add(DbType.Guid, "TEXT");
            _mapping.Add(DbType.Int16, "INTEGER");
            _mapping.Add(DbType.Int32, "INTEGER");
            _mapping.Add(DbType.Int64, "INTEGER");
            _mapping.Add(DbType.Single, "NUMERIC");
            _mapping.Add(DbType.StringFixedLength, "TEXT");
            _mapping.Add(DbType.String, "TEXT");
            _mapping.Add(DbType.Time, "DATETIME");
            _mapping.Add(DbType.Xml, "TEXT");

        }

        public override string Map(DbType dbType)
        {
            if (_mapping.ContainsKey(dbType))
                return _mapping[dbType];
            return null;
        }
    }
}
