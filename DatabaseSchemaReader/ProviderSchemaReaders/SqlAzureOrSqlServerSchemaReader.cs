using System;
using System.Data;

namespace DatabaseSchemaReader.ProviderSchemaReaders
{
    class SqlAzureOrSqlServerSchemaReader : SqlServerSchemaReader
    {
        private const int SqlServerEngine_Azure = 5;

        public SqlAzureOrSqlServerSchemaReader(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
        }

        public override DataTable TableDescription(string tableName)
        {
            if (!IsAzureSqlDatabase)
            {
                return base.TableDescription(tableName);
            }
            //return no data, as table description isn't available
            return CreateDataTable(TableDescriptionCollectionName);
        }

        public override DataTable ColumnDescription(string tableName)
        {
            if (!IsAzureSqlDatabase)
            {
                return base.ColumnDescription(tableName);
            }
            //return no data, as column description isn't available
            return CreateDataTable(ColumnDescriptionCollectionName);
        }

        private bool _haveTestedWhetherServerIsAzure;

        private bool _isAzureSqlDatabase;

        public bool IsAzureSqlDatabase
        {
            get
            {
                if (!_haveTestedWhetherServerIsAzure)
                {
                    using (var conn = Factory.CreateConnection())
                    {
                        if (conn == null)
                        {
                            throw new Exception("Could not connect to database");
                        }

                        conn.ConnectionString = ConnectionString;
                        conn.Open();
                        //an open connection contains a server version
                        //SqlServer 2014 = 12.00.2000
                        //SqlAzure (as of 201407 it's SqlServer 2012) = 11.0.9216.62
                        //SqlServer 2012 SP2 = 11.0.5058.0
                        //SqlServer 2008 R2 SP2 = 10.50.4000.0
                        //2005 = 9.00.5000.00 , 2000 = 8.00.2039
                        int serverVersion;
                        if (!int.TryParse(conn.ServerVersion.Substring(0, 2), out serverVersion))
                        {
                            serverVersion = 9; //SqlServer 2005
                        }
                        if (serverVersion < 11) //before SqlServer 2012, there was no cloud edition
                        {
                            _isAzureSqlDatabase = false;
                        }
                        else
                        {
                            using (var command = conn.CreateCommand())
                            {
                                //Database Engine edition of the instance of SQL Server installed on the server.
                                //1 = Personal or Desktop Engine (Not available for SQL Server 2005.)
                                //2 = Standard (This is returned for Standard and Workgroup.)
                                //3 = Enterprise (This is returned for Enterprise, Enterprise Evaluation, and Developer.)
                                //4 = Express (This is returned for Express, Express Edition with Advanced Services, and Windows Embedded SQL.)
                                //5 = SQL Database
                                //NB: in MONO this returns a SqlVariant, so the CAST is required
                                command.CommandText = "SELECT CAST(SERVERPROPERTY('EngineEdition') AS int)";
                                _isAzureSqlDatabase = (int)command.ExecuteScalar() == SqlServerEngine_Azure;
                            }
                        }
                        _haveTestedWhetherServerIsAzure = true;
                    }
                }

                return _isAzureSqlDatabase;
            }
        }
    }
}