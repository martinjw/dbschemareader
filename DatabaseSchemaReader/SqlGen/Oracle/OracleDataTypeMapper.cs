using System.Collections.Generic;
using System.Data;

namespace DatabaseSchemaReader.SqlGen.Oracle
{
    class OracleDataTypeMapper : DataTypeMapper
    {
        private readonly IDictionary<DbType, string> _mapping = new Dictionary<DbType, string>();

        public OracleDataTypeMapper()
        {
            Init();
        }
        private void Init()
        {
            _mapping.Add(DbType.AnsiStringFixedLength, "CHAR");
            _mapping.Add(DbType.AnsiString, "VARCHAR2");
            _mapping.Add(DbType.Binary, "BLOB");
            _mapping.Add(DbType.Boolean, "NUMBER(1)");
            _mapping.Add(DbType.Byte, "NUMBER(1)");
            _mapping.Add(DbType.Currency, "NUMBER(15,4)");
            _mapping.Add(DbType.Date, "DATE");
            _mapping.Add(DbType.DateTime, "TIMESTAMP");
            _mapping.Add(DbType.DateTime2, "TIMESTAMP");
            _mapping.Add(DbType.DateTimeOffset, "TIMESTAMP");
            _mapping.Add(DbType.Decimal, "NUMBER");
            _mapping.Add(DbType.Double, "DOUBLE");
            _mapping.Add(DbType.Guid, "RAW(16)");
            _mapping.Add(DbType.Int16, "NUMBER(5)");
            _mapping.Add(DbType.Int32, "NUMBER(9)");
            _mapping.Add(DbType.Int64, "NUMBER(15)");
            _mapping.Add(DbType.Single, "REAL");
            _mapping.Add(DbType.StringFixedLength, "NCHAR");
            _mapping.Add(DbType.String, "NVARCHAR2");
            _mapping.Add(DbType.Time, "DATETIME");
            _mapping.Add(DbType.Xml, "XMLTYPE");

        }

        public override string Map(DbType dbType)
        {
            if (_mapping.ContainsKey(dbType))
                return _mapping[dbType];
            return null;
        }
    }
}
