using System.Collections.Generic;
using System.Data;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class MySqlDataTypeMapper : DataTypeMapper
    {
        private readonly IDictionary<DbType, string> _mapping = new Dictionary<DbType, string>();

        public MySqlDataTypeMapper()
        {
            Init();
        }
        private void Init()
        {
            _mapping.Add(DbType.AnsiStringFixedLength, "CHAR");
            _mapping.Add(DbType.AnsiString, "VARCHAR");
            _mapping.Add(DbType.Binary, "BLOB");
            _mapping.Add(DbType.Boolean, "TINYINT");
            _mapping.Add(DbType.Byte, "TINYINT");
            _mapping.Add(DbType.Currency, "DECIMAL"); //could use MONEY
            _mapping.Add(DbType.Date, "DATE");
            _mapping.Add(DbType.DateTime, "DATETIME");
            _mapping.Add(DbType.DateTime2, "DATETIME");
            _mapping.Add(DbType.DateTimeOffset, "DATETIME");
            _mapping.Add(DbType.Decimal, "DECIMAL");
            _mapping.Add(DbType.Double, "DOUBLE");
            _mapping.Add(DbType.Guid, "VARCHAR(64)");
            _mapping.Add(DbType.Int16, "SMALLINT");
            _mapping.Add(DbType.Int32, "INT");
            _mapping.Add(DbType.Int64, "BIGINT");
            _mapping.Add(DbType.Single, "FLOAT");
            _mapping.Add(DbType.StringFixedLength, "CHAR");
            _mapping.Add(DbType.String, "VARCHAR");
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
