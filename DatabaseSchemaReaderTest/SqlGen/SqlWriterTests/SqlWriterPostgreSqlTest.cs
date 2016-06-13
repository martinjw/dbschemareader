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
    public class SqlWriterPostgreSqlTest
    {
        //private const string ProviderName = "Npgsql";
        private const string ProviderName = "Devart.Data.PostgreSql";
        private readonly string _connectionString = ConnectionStrings.PostgreSql;

        private DatabaseTable _table;
        private readonly DbProviderFactory _factory;

        public SqlWriterPostgreSqlTest()
        {
            try
            {
                _factory = DbProviderFactories.GetFactory(ProviderName);
            }
            catch (ArgumentException)
            {
                //not installed. ProviderChecker will assert.inconclusive.
            }
        }

        private DatabaseTable LoadTable()
        {
            if (_table != null) return _table;

            ProviderChecker.Check(ProviderName, _connectionString);

            var dbReader = new DatabaseReader(_connectionString, ProviderName);
            dbReader.Owner = "public"; //otherwise you have "postgres" owned tables and views
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _table = dbReader.Table("country"); //this hits database for columns and constraints
            return _table;
        }

        [TestMethod, TestCategory("Postgresql")]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadTable();

            var runner = new SqlWriterCommonTest(SqlType.PostgreSql, table, _factory, _connectionString);

            runner.RunCountSql();
        }


        [TestMethod, TestCategory("Postgresql")]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadTable();

            var runner = new SqlWriterCommonTest(SqlType.PostgreSql, table, _factory, _connectionString);

            runner.RunSelectAllSql();
        }

        [TestMethod, TestCategory("Postgresql")]
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadTable();

            var runner = new SqlWriterCommonTest(SqlType.PostgreSql, table, _factory, _connectionString);

            runner.RunPagingSql();
        }

        [TestMethod, TestCategory("Postgresql")]
        public void TestGeneratedSqlForPagingStartToEnd()
        {
            var table = LoadTable();

            var runner = new SqlWriterCommonTest(SqlType.PostgreSql, table, _factory, _connectionString);

            runner.RunPagingStartToEndSql();
        }

        [TestMethod, TestCategory("Postgresql")]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            var table = LoadTable();
            var writer = new SqlWriter(table, SqlType.PostgreSql);

            var sql = writer.InsertSqlWithoutOutputParameter();
            Console.WriteLine(sql);
            //int identity;

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
                        //need to set cmd.UnpreparedExecute =true as Protocol 3 doesn't support multiple commands
                        var unpreparedExecute = cmd.GetType().GetProperty("UnpreparedExecute");
                        if (unpreparedExecute != null) unpreparedExecute.SetValue(cmd, true, null);

                        foreach (var column in table.Columns)
                        {
                            if (column.IsAutoNumber) continue;
                            var par = cmd.CreateParameter();
                            par.ParameterName = writer.ParameterName(column.Name);

                            object value = DummyDataCreator.CreateData(column);
                            if (column.Name == "id") value = 9999; //hardcoded for city
                            par.Value = value ?? DBNull.Value;
                            cmd.Parameters.Add(par);
                        }
                        cmd.ExecuteNonQuery();
                        //identity = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    //explicit rollback. If we errored, implicit rollback.
                    transaction.Rollback();
                }
            }

            //assert
            //Assert.AreNotEqual(0, identity);
        }
    }
}
