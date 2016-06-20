using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReaderTest.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlWriterTests
{
    [TestClass]
    public class SqlWriterSqlServerCeTest
    {
        private const string ProviderName = "System.Data.SqlServerCe.4.0";
        private string _connectionString;
        private DatabaseTable _categoriesTable;
        private readonly DbProviderFactory _factory;

        public SqlWriterSqlServerCeTest()
        {
            try
            {
                _factory = DbProviderFactories.GetFactory(ProviderName);

            }
            catch
            {
                //sqlserver not installed
            }
        }

        private DatabaseTable LoadCategoriesFromNorthwind()
        {
            if (_categoriesTable != null) return _categoriesTable;

            if (!File.Exists(ConnectionStrings.SqlServerCeFilePath))
            {
                Assert.Inconclusive("Cannot test SqlServerCe.4.0 as no database file " + ConnectionStrings.SqlServerCeFilePath);
            }
            _connectionString = string.Format(CultureInfo.InvariantCulture, "DataSource=\"{0}\";", ConnectionStrings.SqlServerCeFilePath);
            ProviderChecker.Check(ProviderName, _connectionString);

            var dbReader = new DatabaseReader(_connectionString, ProviderName);
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _categoriesTable = dbReader.Table("Categories"); //this hits database for columns and constraints
            return _categoriesTable;
        }

        [TestMethod, TestCategory("SqlServerCe")]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServerCe, table, _factory, _connectionString);

            runner.RunCountSql();
        }


        [TestMethod, TestCategory("SqlServerCe")]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServerCe, table, _factory, _connectionString);

            runner.RunSelectAllSql();
        }

        [TestMethod, TestCategory("SqlServerCe")]
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadCategoriesFromNorthwind();

            //arrange
            var writer = new SqlWriter(table, SqlType.SqlServerCe);
            var sql = writer.SelectPageSql(); //sane as writer.SelectPageStartToEndRowSql()
            //parameters are offset and pageSize, not the standard ones (limitations of OFFSET/FETCH in sqlserverCe)
            var dataTable = new DataTable();

            //run generated sql
            using (var con = _factory.CreateConnection())
            {
                con.ConnectionString = _connectionString;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var pageSize = cmd.CreateParameter();
                    pageSize.ParameterName = writer.ParameterName("pageSize");
                    pageSize.Value = 2;
                    cmd.Parameters.Add(pageSize);

                    var currentPage = cmd.CreateParameter();
                    currentPage.ParameterName = writer.ParameterName("offset");
                    currentPage.Value = 2;
                    cmd.Parameters.Add(currentPage);

                    var da = _factory.CreateDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dataTable);
                }
            }

            //assert
            Assert.IsTrue(dataTable.Rows.Count > 0, "There should be some categories (this test may fail if database table is empty)");
            Assert.IsTrue(dataTable.Rows.Count <= 4, "Should only return the page size (or less)");
            foreach (var column in table.Columns)
            {
                var name = column.Name;
                Assert.IsTrue(dataTable.Columns.Contains(name), "Should retrieve column " + name);
            }
        }


        [TestMethod, TestCategory("SqlServerCe")]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            var table = LoadCategoriesFromNorthwind();
            var writer = new SqlWriter(table, SqlType.SqlServerCe);
            //can't insert identity, and no output parameters
            var sql = writer.InsertSqlWithoutOutputParameter();
            Console.WriteLine(sql);
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
                        foreach (var column in table.Columns)
                        {
                            if (column.IsAutoNumber)
                                continue;
                            var par = cmd.CreateParameter();
                            par.ParameterName = writer.ParameterName(column.Name);
                            object value = DummyDataCreator.CreateData(column);
                            par.Value = value ?? DBNull.Value;
                            cmd.Parameters.Add(par);
                        }
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = con.CreateCommand())
                    {
                        //can't use SCOPE_IDENTITY in SqlServerCE
                        cmd.CommandText = "SELECT @@IDENTITY;";
                        cmd.Transaction = transaction;
                        //comes back as decimal, but we know it's always an int
                        identity = Convert.ToInt32(cmd.ExecuteScalar());
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
