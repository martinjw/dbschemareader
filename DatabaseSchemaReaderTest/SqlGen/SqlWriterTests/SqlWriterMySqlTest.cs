using System;
using System.Data.Common;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReaderTest.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlWriterTests
{
    [TestClass]
    public class SqlWriterMySqlTest
    {
        private const string ProviderName = "MySql.Data.MySqlClient";
        private readonly string _connectionString = ConnectionStrings.MySql;
        private DatabaseTable _categoriesTable;
        private readonly DbProviderFactory _factory;

        public SqlWriterMySqlTest()
        {
            try
            {
                _factory = DbProviderFactories.GetFactory(ProviderName);
            }
            catch (ArgumentException)
            {
                //MySQL is not installed. ProviderChecker will assert.inconclusive.
            }
            catch (System.Configuration.ConfigurationErrorsException)
            {
                //MySQL is not installed. ProviderChecker will assert.inconclusive.
            }
        }

        private DatabaseTable LoadCountryFromSakila()
        {
            if (_categoriesTable != null) return _categoriesTable;

            ProviderChecker.Check(ProviderName, _connectionString);

            var dbReader = new DatabaseReader(_connectionString, ProviderName);
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _categoriesTable = dbReader.Table("country"); //this hits database for columns and constraints
            return _categoriesTable;
        }

        [TestMethod, TestCategory("MySql")]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadCountryFromSakila();

            var runner = new SqlWriterCommonTest(SqlType.MySql, table, _factory, _connectionString);

            runner.RunCountSql();
        }


        [TestMethod, TestCategory("MySql")]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadCountryFromSakila();

            var runner = new SqlWriterCommonTest(SqlType.MySql, table, _factory, _connectionString);

            runner.RunSelectAllSql();
        }

        [TestMethod, TestCategory("MySql")]
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadCountryFromSakila();

            var runner = new SqlWriterCommonTest(SqlType.MySql, table, _factory, _connectionString);

            runner.RunPagingSql();
        }

        [TestMethod, TestCategory("MySql")]
        public void TestGeneratedSqlForPagingStartToEnd()
        {
            var table = LoadCountryFromSakila();

            var runner = new SqlWriterCommonTest(SqlType.MySql, table, _factory, _connectionString);

            runner.RunPagingStartToEndSql();
        }

        [TestMethod, TestCategory("MySql")]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            var table = LoadCountryFromSakila();
            var writer = new SqlWriter(table, SqlType.MySql);
            //MySQL can only use output parameters with sprocs.
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
                            if (column.IsAutoNumber) continue;
                            var par = cmd.CreateParameter();
                            par.ParameterName = writer.ParameterName(column.Name);

                            object value = DummyDataCreator.CreateData(column);
                            par.Value = value ?? DBNull.Value;
                            cmd.Parameters.Add(par);
                        }
                        identity = Convert.ToInt32(cmd.ExecuteScalar());
                        //if using a sproc
                        //identity = (int)cmd.Parameters[identityParameterName].Value;
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
