using System;
using System.Data.Common;
using System.IO;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReaderTest.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlWriterTests
{
    [TestClass]
    public class SqlWriterSQLiteTest
    {
        private const string ProviderName = "System.Data.SQLite";
        private readonly string _databaseFile = ConnectionStrings.SqLiteFilePath;
        private readonly string _connectionString;
        private DatabaseTable _categoriesTable;
        private readonly DbProviderFactory _factory;

        public SqlWriterSQLiteTest()
        {
            _connectionString = "Data Source=" + _databaseFile;
            try
            {
                _factory = DbProviderFactories.GetFactory(ProviderName);
            }
            catch (ArgumentException)
            {
                //SQLite is not installed. ProviderChecker will assert.inconclusive.
            }
            catch (System.Configuration.ConfigurationException)
            {
                //SQLite is not installed. ProviderChecker will assert.inconclusive.
            }
        }

        private DatabaseTable LoadCategoriesFromNorthwind()
        {
            if (_categoriesTable != null) return _categoriesTable;

            if (!File.Exists(_databaseFile))
                Assert.Inconclusive("SQLite database file not found: " + _databaseFile);

            ProviderChecker.Check(ProviderName, _connectionString);

            var dbReader = new DatabaseReader(_connectionString, ProviderName);
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _categoriesTable = dbReader.Table("Categories"); //this hits database for columns and constraints
            if (_categoriesTable == null)
                Assert.Inconclusive("Could not load Categories table from SQLite file");
            return _categoriesTable;
        }

        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SQLite, table, _factory, _connectionString);

            runner.RunCountSql();
        }


        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SQLite, table, _factory, _connectionString);

            runner.RunSelectAllSql();
        }

        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SQLite, table, _factory, _connectionString);

            runner.RunPagingSql();
        }

        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForPagingStartToEnd()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SQLite, table, _factory, _connectionString);

            runner.RunPagingStartToEndSql();
        }

        [TestMethod, TestCategory("SQLite")]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            var table = LoadCategoriesFromNorthwind();
            var writer = new SqlWriter(table, SqlType.SQLite);
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
                        foreach (var column in table.Columns)
                        {
                            if (column.IsAutoNumber) continue;

                            var par = cmd.CreateParameter();
                            par.ParameterName = writer.ParameterName(column.Name);
                            object value = DummyDataCreator.CreateData(column);
                            par.Value = value ?? DBNull.Value;
                            cmd.Parameters.Add(par);
                        }
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
