using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    static class SqlFormatFactory
    {
        public static ISqlFormatProvider Provider(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.Oracle:
                    return new Oracle.SqlFormatProvider();
                case SqlType.MySql:
                    return new MySql.SqlFormatProvider();
                case SqlType.SQLite:
                    return new SqLite.SqlFormatProvider();
                case SqlType.PostgreSql:
                    return new PostgreSql.SqlFormatProvider();
                case SqlType.Db2:
                    return new Db2.SqlFormatProvider();
                case SqlType.SqlServerCe:
                    return new SqlServerCe.SqlServerCeFormatProvider();
                default:
                    return new SqlServer.SqlFormatProvider();
            }
        }
    }
}
