using System;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
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

namespace DatabaseSchemaReaderTest.SqlGen
{
    [TestClass]
    public class SqlWriterTest
    {
        private const string ProviderName = "System.Data.SqlClient";
        private const string ConnectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
        private DatabaseTable _categoriesTable;

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
            //arrange
            var table = LoadCategoriesFromNorthwind();
            var writer = new SqlWriter(table, SqlType.SqlServer);
            var sql = writer.CountSql();
            var factory = DbProviderFactories.GetFactory(ProviderName);
            int count;

            //run generated sql
            using (var con = factory.CreateConnection())
            {
                con.ConnectionString = ConnectionString;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    con.Open();
                    count = (int)cmd.ExecuteScalar();
                }
            }

            //assert
            Assert.IsTrue(count > 0, "There should be some categories (this test may fail if database table is empty)");
        }


        [TestMethod]
        public void TestGeneratedSqlForSelectAll()
        {
            //arrange
            var table = LoadCategoriesFromNorthwind();
            var writer = new SqlWriter(table, SqlType.SqlServer);
            var sql = writer.SelectAllSql();
            var factory = DbProviderFactories.GetFactory(ProviderName);
            var dataTable = new DataTable();

            //run generated sql
            using (var con = factory.CreateConnection())
            {
                con.ConnectionString = ConnectionString;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var da = factory.CreateDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dataTable);
                }
            }

            //assert
            Assert.IsTrue(dataTable.Rows.Count > 0, "There should be some categories (this test may fail if database table is empty)");
            foreach (var column in table.Columns)
            {
                var name = column.Name;
                Assert.IsTrue(dataTable.Columns.Contains(name), "Should retrieve column " + name);
            }
        }

        [TestMethod]
        public void TestGeneratedSqlForPaging()
        {
            //arrange
            var table = LoadCategoriesFromNorthwind();
            var writer = new SqlWriter(table, SqlType.SqlServer);
            var sql = writer.SelectPageSql();
            var factory = DbProviderFactories.GetFactory(ProviderName);
            var dataTable = new DataTable();

            //run generated sql
            using (var con = factory.CreateConnection())
            {
                con.ConnectionString = ConnectionString;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var pageSize = factory.CreateParameter();
                    pageSize.ParameterName = "@pageSize";
                    pageSize.Value = 4;
                    cmd.Parameters.Add(pageSize);

                    var currentPage = factory.CreateParameter();
                    currentPage.ParameterName = "@currentPage";
                    currentPage.Value = 1;
                    cmd.Parameters.Add(currentPage);


                    var da = factory.CreateDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dataTable);
                }
            }

            //assert
            Assert.IsTrue(dataTable.Rows.Count > 0, "There should be some categories (this test may fail if database table is empty)");
            foreach (var column in table.Columns)
            {
                var name = column.Name;
                Assert.IsTrue(dataTable.Columns.Contains(name), "Should retrieve column " + name);
            }
        }

        [TestMethod]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            var table = LoadCategoriesFromNorthwind();
            var writer = new SqlWriter(table, SqlType.SqlServer);
            var sql = writer.InsertSql();
            var factory = DbProviderFactories.GetFactory(ProviderName);

            //run generated sql
            using (var con = factory.CreateConnection())
            {
                con.ConnectionString = ConnectionString;
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = sql;
                        cmd.Transaction = transaction;
                        foreach (var column in table.Columns)
                        {
                            var par = cmd.CreateParameter();

                            //should be parameter name
                            par.ParameterName = writer.ParameterName(column.Name);
                            if (column.IsIdentity)
                            {
                                par.Direction = ParameterDirection.Output;
                                par.DbType = DbType.Int32;
                            }
                            else
                            {
                                if (column.DataType.IsDateTime)
                                    par.Value = DateTime.Now;
                                else if (column.DataType.IsNumeric)
                                    par.Value = 1;
                                else if (column.DataType.IsString)
                                {
                                    var length = column.Length.GetValueOrDefault(); //could be a clob, -1
                                    par.Value = Guid.NewGuid().ToString("N").Substring(0, length < 1 ? 32 : length);
                                }
                                else if (column.DataType.GetNetType() == typeof(byte[]))
                                {
                                    par.Value = new byte[] {};
                                }
                                else
                                    par.Value = DBNull.Value;
                            }
                            cmd.Parameters.Add(par);
                        }
                        cmd.ExecuteNonQuery();
                    }

                    //explicit rollback. If we errored, implicit rollback.
                    transaction.Rollback();
                }
            }

            //assert
        }
    }
}
