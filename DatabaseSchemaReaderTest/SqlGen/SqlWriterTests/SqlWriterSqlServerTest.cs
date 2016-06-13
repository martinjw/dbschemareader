using System;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReaderTest.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlWriterTests
{
    [TestClass]
    public class SqlWriterSqlServerTest
    {
        private const string ProviderName = "System.Data.SqlClient";
        readonly string _connectionString = ConnectionStrings.Northwind;
        private DatabaseTable _categoriesTable;
        private readonly DbProviderFactory _factory;

        public SqlWriterSqlServerTest()
        {
            _factory = DbProviderFactories.GetFactory(ProviderName);
        }

        private DatabaseTable LoadCategoriesFromNorthwind()
        {
            if (_categoriesTable != null) return _categoriesTable;

            ProviderChecker.Check(ProviderName, _connectionString);

            var dbReader = new DatabaseReader(_connectionString, ProviderName);
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _categoriesTable = dbReader.Table("Categories"); //this hits database for columns and constraints
            return _categoriesTable;
        }

        [TestMethod, TestCategory("SqlServer")]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServer, table, _factory, _connectionString);

            runner.RunCountSql();
        }


        [TestMethod, TestCategory("SqlServer")]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServer, table, _factory, _connectionString);

            runner.RunSelectAllSql();
        }

        [TestMethod, TestCategory("SqlServer")]
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServer, table, _factory, _connectionString);

            runner.RunPagingSql();
        }

        [TestMethod, TestCategory("SqlServer")]
        public void TestGeneratedSqlForPagingStartToEnd()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServer, table, _factory, _connectionString);

            runner.RunPagingStartToEndSql();
        }

        [TestMethod, TestCategory("SqlServer")]
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
                con.ConnectionString = _connectionString;
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
                            if (column.IsAutoNumber)
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
