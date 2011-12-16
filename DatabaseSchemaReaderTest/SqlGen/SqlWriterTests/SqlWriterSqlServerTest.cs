using System;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReaderTest.IntegrationTests;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
#endif

namespace DatabaseSchemaReaderTest.SqlGen.SqlWriterTests
{
    [TestClass]
    public class SqlWriterSqlServerTest
    {
        private const string ProviderName = "System.Data.SqlClient";
        const string ConnectionString = ConnectionStrings.Northwind;
        private DatabaseTable _categoriesTable;
        private readonly DbProviderFactory _factory;

        public SqlWriterSqlServerTest()
        {
            _factory = DbProviderFactories.GetFactory(ProviderName);
        }

        private DatabaseTable LoadCategoriesFromNorthwind()
        {
            if (_categoriesTable != null) return _categoriesTable;

            ProviderChecker.Check(ProviderName, ConnectionString);

            var dbReader = new DatabaseReader(ConnectionString, ProviderName);
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _categoriesTable = dbReader.Table("Categories"); //this hits database for columns and constraints
            return _categoriesTable;
        }

        [TestMethod]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServer, table, _factory, ConnectionString);

            runner.RunCountSql();
        }


        [TestMethod]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServer, table, _factory, ConnectionString);

            runner.RunSelectAllSql();
        }

        [TestMethod]
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServer, table, _factory, ConnectionString);

            runner.RunPagingSql();
        }

        [TestMethod]
        public void TestGeneratedSqlForPagingStartToEnd()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServer, table, _factory, ConnectionString);

            runner.RunPagingStartToEndSql();
        }

        [TestMethod]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            var table = LoadCategoriesFromNorthwind();
            var writer = new SqlWriter(table, SqlType.SqlServer);
            var sql = writer.InsertSql();
            int identity;

            //run generated sql
            using (var con = _factory.CreateConnection())
            {
                con.ConnectionString = ConnectionString;
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.Transaction = transaction;
                        string identityParameterName = "Id";
                        foreach (var column in table.Columns)
                        {
                            var par = cmd.CreateParameter();
                            par.ParameterName = writer.ParameterName(column.Name);
                            if (column.IsIdentity)
                            {
                                //get the name of the identity parameter
                                identityParameterName = par.ParameterName;
                                par.Direction = ParameterDirection.Output;
                                par.DbType = DbType.Int32;
                            }
                            else
                            {
                                object value = DummyDataCreator.CreateData(column);
                                par.Value = value ?? DBNull.Value;
                            }
                            cmd.Parameters.Add(par);
                        }
                        cmd.ExecuteNonQuery();
                        identity = (int)cmd.Parameters[identityParameterName].Value;
                    }

                    //explicit rollback. If we errored, implicit rollback.
                    transaction.Rollback();
                }
            }

            //assert
            Assert.AreNotEqual(0, identity);
        }
    }
}
