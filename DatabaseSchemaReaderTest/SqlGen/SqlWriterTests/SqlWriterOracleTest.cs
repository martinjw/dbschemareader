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
    public class SqlWriterOracleTest
    {
        private const string ProviderName = "System.Data.OracleClient";
        const string ConnectionString = ConnectionStrings.OracleHr;
        private DatabaseTable _regionsTable;
        private readonly DbProviderFactory _factory;

        public SqlWriterOracleTest()
        {
            _factory = DbProviderFactories.GetFactory(ProviderName);
        }

        private DatabaseTable LoadRegionsFromHr()
        {
            if (_regionsTable != null) return _regionsTable;

            ProviderChecker.Check(ProviderName, ConnectionString);

            var dbReader = new DatabaseReader(ConnectionString, ProviderName);
            dbReader.Owner = "HR";
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _regionsTable = dbReader.Table("REGIONS"); //this hits database for columns and constraints
            return _regionsTable;
        }

        [TestMethod]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadRegionsFromHr();

            var runner = new SqlWriterCommonTest(SqlType.Oracle, table, _factory, ConnectionString);
            
            runner.RunCountSql();
        }


        [TestMethod]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadRegionsFromHr();

            var runner = new SqlWriterCommonTest(SqlType.Oracle, table, _factory, ConnectionString);

            runner.RunSelectAllSql();
        }

        [TestMethod]
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadRegionsFromHr();

            var runner = new SqlWriterCommonTest(SqlType.Oracle, table, _factory, ConnectionString);

            runner.RunPagingSql();
        }

        [TestMethod]
        public void TestGeneratedSqlForPagingStartToEnd()
        {
            var table = LoadRegionsFromHr();

            var runner = new SqlWriterCommonTest(SqlType.Oracle, table, _factory, ConnectionString);

            runner.RunPagingStartToEndSql();
        }

        [TestMethod]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            var table = LoadRegionsFromHr();
            var writer = new SqlWriter(table, SqlType.Oracle);
            var sql = writer.InsertSql();

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
                        foreach (var column in table.Columns)
                        {
                            var par = cmd.CreateParameter();
                            par.ParameterName = writer.ParameterName(column.Name);
                            if (column.IsIdentity)
                            {
                                //we could be using sequences here
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
                    }

                    //explicit rollback. If we errored, implicit rollback.
                    transaction.Rollback();
                }
            }

            //assert
        }
    }
}
