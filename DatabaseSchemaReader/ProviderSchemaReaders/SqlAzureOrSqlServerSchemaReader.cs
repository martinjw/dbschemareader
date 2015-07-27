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
                        var serverVersion = SqlServerVersion(conn);

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