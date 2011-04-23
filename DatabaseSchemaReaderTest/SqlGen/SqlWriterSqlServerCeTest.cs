using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
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

namespace DatabaseSchemaReaderTest.SqlGen
{
    [TestClass]
    public class SqlWriterSqlServerCeTest
    {
        private const string ProviderName = "System.Data.SqlServerCe.4.0";
        private const string FilePath = @"C:\Data\northwind.sdf";
        private string _connectionString;
        private DatabaseTable _categoriesTable;
        private readonly DbProviderFactory _factory;

        public SqlWriterSqlServerCeTest()
        {
            _factory = DbProviderFactories.GetFactory(ProviderName);
        }

        private DatabaseTable LoadCategoriesFromNorthwind()
        {
            if (_categoriesTable != null) return _categoriesTable;

            if (!File.Exists(FilePath))
            {
                Assert.Inconclusive("Cannot test SqlServerCe.4.0 as no database file " + FilePath);
            }
            _connectionString = string.Format(CultureInfo.InvariantCulture, "DataSource=\"{0}\";", FilePath);
            ProviderChecker.Check(ProviderName, _connectionString);

            var dbReader = new DatabaseReader(_connectionString, ProviderName);
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _categoriesTable = dbReader.Table("Categories"); //this hits database for columns and constraints
            return _categoriesTable;
        }

        [TestMethod]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServerCe, table, _factory, _connectionString);

            runner.RunCountSql();
        }


        [TestMethod]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServerCe, table, _factory, _connectionString);

            runner.RunSelectAllSql();
        }

        //[TestMethod] TODO
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServerCe, table, _factory, _connectionString);

            runner.RunPagingSql();
        }

        //[TestMethod] TODO
        public void TestGeneratedSqlForPagingStartToEnd()
        {
            var table = LoadCategoriesFromNorthwind();

            var runner = new SqlWriterCommonTest(SqlType.SqlServerCe, table, _factory, _connectionString);

            runner.RunPagingStartToEndSql();
        }

        [TestMethod]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            var table = LoadCategoriesFromNorthwind();
            var writer = new SqlWriter(table, SqlType.SqlServerCe);
            //can't insert identity, and no output parameters
            var sql = writer.InsertSqlWithoutOutputParameter();
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
                            if (column.IsIdentity)
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
