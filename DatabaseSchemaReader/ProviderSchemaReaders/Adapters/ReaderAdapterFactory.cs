using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Adapters
{
    static class ReaderAdapterFactory
    {
        public static ReaderAdapter Create(SchemaParameters schemaParameters)
        {
            ReaderAdapter schemaReader = null;
            switch (schemaParameters.SqlType)
            {
                case SqlType.SqlServer:
                    return new SqlServerAdapter(schemaParameters);
                case SqlType.Oracle:
                    schemaReader = new OracleAdapter(schemaParameters);
                    break;
                case SqlType.SQLite:
                    schemaReader = new SqLiteAdapter(schemaParameters);
                    break;               
                case SqlType.PostgreSql:
                    schemaReader = new PostgreSqlAdapter(schemaParameters);
                    break;
                //case SqlType.SqlServerCe:
                //    schemaReader = new SqlServerCeAdapter(schemaParameters);
                //    break;
                //case SqlType.MySql:
                //    schemaReader = new MySqlAdapter(schemaParameters);
                //    break;
                default:
                    //var providerName = schemaParameters.ProviderName;
                    //all the other types
                    //if (providerName.Equals("Ingres.Client", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new IngresSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("iAnyWhere.Data.SQLAnyWhere", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new SybaseAsaSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("Sybase.Data.AseClient", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new SybaseAseSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("iAnyWhere.Data.UltraLite", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new SybaseUltraLiteSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("System.Data.OleDb", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new OleDbSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("System.Data.VistaDB", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new VistaDbSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("IBM.Data.DB2.iSeries", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new Db2ISeriesSchemaReader(connectionString, providerName);
                    //}
                    //else if (providerName.Equals("FirebirdSql.Data.FirebirdClient", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    schemaReader = new FirebirdSqlSchemaReader(connectionString, providerName);
                    //}

                    break;
            }
            if (schemaReader == null)
            {
#if !NETSTANDARD1_5
                schemaReader = new DbProviderReaderAdapter(schemaParameters);
#else
                schemaReader = new ReaderAdapter(schemaParameters);
#endif
            }
            return schemaReader;
        }
    }
}
