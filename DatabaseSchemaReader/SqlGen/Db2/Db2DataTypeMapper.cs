using System.Collections.Generic;
using System.Data;

namespace DatabaseSchemaReader.SqlGen.Db2
{
    class Db2DataTypeMapper : DataTypeMapper
    {
        private readonly IDictionary<DbType, string> _mapping = new Dictionary<DbType, string>();

        public Db2DataTypeMapper()
        {
            Init();
        }
        private void Init()
        {
            _mapping.Add(DbType.AnsiStringFixedLength, "CHAR");
            _mapping.Add(DbType.AnsiString, "VARCHAR");
            _mapping.Add(DbType.Binary, "VARGRAPHIC");
            _mapping.Add(DbType.Boolean, "SMALLINT");
            _mapping.Add(DbType.Byte, "VARGRAPHIC");
            _mapping.Add(DbType.Currency, "DECIMAL(19,4)");
            _mapping.Add(DbType.Date, "DATE");
            _mapping.Add(DbType.DateTime, "TIMESTAMP");
            _mapping.Add(DbType.DateTime2, "TIMESTAMP");
            _mapping.Add(DbType.DateTimeOffset, "TIMESTAMP");
            _mapping.Add(DbType.Decimal, "DECIMAL");
            _mapping.Add(DbType.Double, "DOUBLE");
            _mapping.Add(DbType.Guid, "CHAR(16) FOR BIT DATA");
            _mapping.Add(DbType.Int16, "SMALLINT");
            _mapping.Add(DbType.Int32, "INTEGER");
            _mapping.Add(DbType.Int64, "BIGINT");
            _mapping.Add(DbType.Single, "REAL");
            _mapping.Add(DbType.StringFixedLength, "CHAR");
            _mapping.Add(DbType.String, "VARCHAR");
            _mapping.Add(DbType.Time, "TIME");
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
