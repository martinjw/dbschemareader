using System;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Filters;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class SchemaParameters
    {
//#if COREFX
        public SchemaParameters(System.Data.Common.DbConnection dbConnection)
        {
            DbConnection = dbConnection;
            ProviderName = DbConnection.GetType().Namespace;
            ConnectionString = dbConnection.ConnectionString;
            SqlType = ProviderToSqlType.Convert(ProviderName);
            Exclusions = new Exclusions();
        }

        public SchemaParameters(System.Data.Common.DbTransaction dbTransaction) :this(dbTransaction.Connection)
        {
            DbTransaction = dbTransaction;
        }
        
        public System.Data.Common.DbConnection DbConnection { get; private set; }

        public System.Data.Common.DbTransaction DbTransaction { get; private set; }
//#else
        public SchemaParameters(string connectionString, SqlType sqlType)
        {
            ConnectionString = connectionString;
            ProviderName = ProviderToSqlType.Convert(sqlType);
            SqlType = sqlType;
            Exclusions = new Exclusions();
        }

        public SchemaParameters(string connectionString, string provider)
        {
            ConnectionString = connectionString;
            ProviderName = provider;
            SqlType = ProviderToSqlType.Convert(ProviderName);
            Exclusions = new Exclusions();
        }
//#endif

        public string ConnectionString { get; private set; }
		
        public SqlType? SqlType { get; private set; }

        /// <summary>
        /// Exclude specified items when reading schema
        /// </summary>
        /// <value>
        /// The exclusions.
        /// </value>
        public Exclusions Exclusions { get; private set; }

        /// <summary>
        /// Gets or sets the owner (for Oracle) /schema (for SqlServer) / database (MySql). Always set it with Oracle; if you use other than dbo in SqlServer you should also set it.
        /// If it is null or empty, all owners are returned.
        /// </summary>
        public string Owner { get; set; }

        public string ProviderName { get; private set; }

        public DatabaseSchema DatabaseSchema { get; set; }
    }
}
