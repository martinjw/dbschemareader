using System.Collections.Generic;
using System.Data;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    sealed class SqlServerDataTypeMapper : DataTypeMapper
    {
        private readonly IDictionary<DbType, string> _mapping = new Dictionary<DbType, string>();
        
        public SqlServerDataTypeMapper()
        {
            Init();
        }

        private void Init()
        {
            //based on MetaType.GetMetaTypeFromDbType
            _mapping.Add(DbType.AnsiStringFixedLength, "CHAR");
            _mapping.Add(DbType.AnsiString, "VARCHAR");
            _mapping.Add(DbType.Binary, "VARBINARY");
            _mapping.Add(DbType.Boolean, "BIT");
            _mapping.Add(DbType.Byte, "TINYINT");
            _mapping.Add(DbType.Currency, "MONEY");
            _mapping.Add(DbType.Date, "DATE");
            _mapping.Add(DbType.DateTime, "DATETIME");
            _mapping.Add(DbType.DateTime2, "DATETIME2");
            _mapping.Add(DbType.DateTimeOffset, "DATETIMEOFFSET");
            _mapping.Add(DbType.Decimal, "DECIMAL");
            _mapping.Add(DbType.Double, "FLOAT");
            _mapping.Add(DbType.Guid, "UNIQUEIDENTIFIER");
            _mapping.Add(DbType.Int16, "SMALLINT");
            _mapping.Add(DbType.Int32, "INT");
            _mapping.Add(DbType.Int64, "BIGINT");
            _mapping.Add(DbType.Single, "REAL");
            _mapping.Add(DbType.StringFixedLength, "NCHAR");
            _mapping.Add(DbType.String, "NVARCHAR");
            _mapping.Add(DbType.Time, "TIME");
            _mapping.Add(DbType.Xml, "XML");

        }

        public override string Map(DbType dbType)
        {
            if (_mapping.ContainsKey(dbType))
                return _mapping[dbType];
            return null;
        }
    }
}
