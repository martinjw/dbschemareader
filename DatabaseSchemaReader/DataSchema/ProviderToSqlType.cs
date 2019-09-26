using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Converts a provider invariant name to a SqlType
    /// </summary>
    public static class ProviderToSqlType
    {
        /// <summary>
        /// Converts the specified provider name to a <see cref="SqlType"/> or null if unknown.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        public static SqlType? Convert(string providerName)
        {
            if (string.IsNullOrEmpty(providerName)) return null;

            if (providerName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
                return SqlType.SqlServer;
            if (providerName.Equals("Microsoft.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
                return SqlType.SqlServer;
            if (providerName.IndexOf("SQLite", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return SqlType.SQLite;
            }
            if (providerName.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return SqlType.Oracle;
            }
            if (providerName.IndexOf("MySql", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return SqlType.MySql;
            }
            if (providerName.Equals("System.Data.SqlServerCe.4.0", StringComparison.OrdinalIgnoreCase))
                return SqlType.SqlServerCe;
            if (providerName.Equals("Npgsql", StringComparison.OrdinalIgnoreCase) || 
                providerName.Equals("Devart.Data.PostgreSql", StringComparison.OrdinalIgnoreCase))
                return SqlType.PostgreSql;
            if (providerName.Equals("IBM.Data.DB2", StringComparison.OrdinalIgnoreCase))
                return SqlType.Db2;

            //could be something we don't have a direct syntax for
            return null;
        }

        /// <summary>
        /// Converts the specified SQL type into the most common provider.
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        /// <returns></returns>
        public static string Convert(SqlType sqlType)
        {
            switch (sqlType)
            {
                case SqlType.SqlServer:
                    return "System.Data.SqlClient";
                case SqlType.Oracle:
                    return "System.Data.OracleClient";
                case SqlType.MySql:
                    return "MySql.Data.MySqlClient";
                case SqlType.SQLite:
                    return "System.Data.SQLite";
                case SqlType.SqlServerCe:
                    return "System.Data.SqlServerCe.4.0";
                case SqlType.PostgreSql:
                    return "Npgsql";
                case SqlType.Db2:
                    return "IBM.Data.DB2";
                default:
                    return null;
            }
        }
    }
}
