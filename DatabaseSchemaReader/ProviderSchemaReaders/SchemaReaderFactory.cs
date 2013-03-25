using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    static class SchemaReaderFactory
    {
        public static SchemaExtendedReader Create(string connectionString, string providerName)
        {
            if (string.IsNullOrEmpty(providerName))
                throw new ArgumentNullException("providerName", "providerName must not be empty");

            SchemaExtendedReader schemaReader = null;
            var type = ProviderToSqlType.Convert(providerName);
            switch (type)
            {
                case SqlType.Oracle:
                    schemaReader = new OracleSchemaReader(connectionString, providerName);
                    break;
                case SqlType.SqlServer:
                    schemaReader = new SqlServerSchemaReader(connectionString, providerName);
                    break;
                case SqlType.SqlServerCe:
                    schemaReader = new SqlServerCeSchemaReader(connectionString, providerName);
                    break;
                case SqlType.MySql:
                    schemaReader = new MySqlSchemaReader(connectionString, providerName);
                    break;
                case SqlType.PostgreSql:
                    schemaReader = new PostgreSqlSchemaReader(connectionString, providerName);
                    break;
                case SqlType.Db2:
                    schemaReader = new Db2SchemaReader(connectionString, providerName);
                    break;
                default:
                    //all the other types
                    if (providerName.Equals("Ingres.Client", StringComparison.OrdinalIgnoreCase))
                    {
                        schemaReader = new IngresSchemaReader(connectionString, providerName);
                    }
                    else if (providerName.Equals("iAnyWhere.Data.SQLAnyWhere", StringComparison.OrdinalIgnoreCase))
                    {
                        schemaReader = new SybaseAsaSchemaReader(connectionString, providerName);
                    }
                    else if (providerName.Equals("Sybase.Data.AseClient", StringComparison.OrdinalIgnoreCase))
                    {
                        schemaReader = new SybaseAseSchemaReader(connectionString, providerName);
                    }
                    else if (providerName.Equals("iAnyWhere.Data.UltraLite", StringComparison.OrdinalIgnoreCase))
                    {
                        schemaReader = new SybaseUltraLiteSchemaReader(connectionString, providerName);
                    }
                    else if (providerName.Equals("System.Data.OleDb", StringComparison.OrdinalIgnoreCase))
                    {
                        schemaReader = new OleDbSchemaReader(connectionString, providerName);
                    }
                    else if (providerName.Equals("System.Data.VistaDB", StringComparison.OrdinalIgnoreCase))
                    {
                        schemaReader = new VistaDbSchemaReader(connectionString, providerName);
                    }

                    break;
            }
            if (schemaReader == null)
            {
                schemaReader = new SchemaExtendedReader(connectionString, providerName);
            }
            return schemaReader;
        }

        public static SchemaExtendedReader Create(string connectionString, SqlType sqlType)
        {
            SchemaExtendedReader schemaReader;
            switch (sqlType)
            {
                case SqlType.Oracle:
                    schemaReader = new OracleSchemaReader(connectionString, "System.Data.OracleClient");
                    break;
                case SqlType.SqlServer:
                    schemaReader = new SqlServerSchemaReader(connectionString, "System.Data.SqlClient");
                    break;
                case SqlType.SqlServerCe:
                    schemaReader = new SqlServerCeSchemaReader(connectionString, "System.Data.SqlServerCe.4.0");
                    break;
                case SqlType.MySql:
                    schemaReader = new MySqlSchemaReader(connectionString, "MySql.Data.MySqlClient");
                    break;
                case SqlType.PostgreSql:
                    schemaReader = new PostgreSqlSchemaReader(connectionString, "Npgsql");
                    break;
                case SqlType.Db2:
                    schemaReader = new Db2SchemaReader(connectionString, "IBM.Data.DB2");
                    break;
                case SqlType.SQLite:
                    schemaReader = new SchemaExtendedReader(connectionString, "System.Data.SQLite");
                    break;
                default:
                    throw new ArgumentOutOfRangeException("sqlType", "Not a recognized SqlType");
            }
            return schemaReader;
        }
    }
}
