using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion
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
            if (providerName.Equals("System.Data.SQLite", StringComparison.OrdinalIgnoreCase))
                return SqlType.SQLite;
            if (providerName.IndexOf("Oracle", StringComparison.OrdinalIgnoreCase) != -1)
                return SqlType.Oracle;
            if (providerName.Equals("MySql.Data.MySqlClient", StringComparison.OrdinalIgnoreCase))
                return SqlType.MySql;
            if (providerName.Equals("Devart.Data.MySql", StringComparison.OrdinalIgnoreCase))
                return SqlType.MySql;
            if (providerName.Equals("System.Data.SqlServerCe.4.0", StringComparison.OrdinalIgnoreCase))
                return SqlType.SqlServerCe;
            if (providerName.Equals("System.Data.SqlClient", StringComparison.OrdinalIgnoreCase))
                return SqlType.SqlServer;
            if (providerName.Equals("Npgsql", StringComparison.OrdinalIgnoreCase) || 
                providerName.Equals("Devart.Data.PostgreSql", StringComparison.OrdinalIgnoreCase))
                return SqlType.PostgreSql;
            if (providerName.Equals("IBM.Data.DB2", StringComparison.OrdinalIgnoreCase))
                return SqlType.Db2;

            //could be something we don't have a direct syntax for
            return null;
        }
    }
}
