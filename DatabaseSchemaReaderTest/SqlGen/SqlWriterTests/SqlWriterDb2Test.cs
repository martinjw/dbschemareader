using System;
using System.Configuration;
using System.Data.Common;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReaderTest.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlWriterTests
{
    [TestClass]
    public class SqlWriterDb2Test
    {
        const string ProviderName = "IBM.Data.DB2";
        const string ConnectionString = @"Server=localhost:50000;UID=db2admin;pwd=db2;Database=Sample";

        //db2 "grant dbadm on database to user db2admin"

        private DatabaseTable _table;
        private readonly DbProviderFactory _factory;

        public SqlWriterDb2Test()
        {
            try
            {
                _factory = DbProviderFactories.GetFactory(ProviderName);
            }
            catch (ConfigurationErrorsException)
            {
                //not installed. ProviderChecker will assert.inconclusive.
            }
            catch (ArgumentException)
            {
                //not installed. ProviderChecker will assert.inconclusive.
            }
        }

        private DatabaseTable LoadTable()
        {
            if (_table != null) return _table;

            ProviderChecker.Check(ProviderName, ConnectionString);

            var dbReader = new DatabaseReader(ConnectionString, ProviderName);
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            _table = dbReader.Table("STAFF"); //this hits database for columns and constraints
            return _table;
        }

        [TestMethod, TestCategory("DB2")]
        public void TestGeneratedSqlForCount()
        {
            var table = LoadTable();

            var runner = new SqlWriterCommonTest(SqlType.Db2, table, _factory, ConnectionString);

            runner.RunCountSql();
        }


        [TestMethod, TestCategory("DB2")]
        public void TestGeneratedSqlForSelectAll()
        {
            var table = LoadTable();

            var runner = new SqlWriterCommonTest(SqlType.Db2, table, _factory, ConnectionString);

            runner.RunSelectAllSql();
        }

        [TestMethod, TestCategory("DB2")]
        public void TestGeneratedSqlForPaging()
        {
            var table = LoadTable();

            var runner = new SqlWriterCommonTest(SqlType.Db2, table, _factory, ConnectionString);

            runner.RunPagingSql();
        }

        [TestMethod, TestCategory("DB2")]
        public void TestGeneratedSqlForPagingStartToEnd()
        {
            var table = LoadTable();

            var runner = new SqlWriterCommonTest(SqlType.Db2, table, _factory, ConnectionString);

            runner.RunPagingStartToEndSql();
        }

        //[TestMethod]
        public void TestGeneratedSqlForInsert()
        {
            //arrange
            ProviderChecker.Check(ProviderName, ConnectionString);
            var dbReader = new DatabaseReader(ConnectionString, ProviderName);
            dbReader.DataTypes(); //ensure we have datatypes (this doesn't hit the database)
            var table = dbReader.Table("TABWITHIDENTITY"); //this hits database for columns and constraints

            var writer = new SqlWriter(table, SqlType.Db2);

            var sql = writer.InsertSqlWithoutOutputParameter();
            Console.WriteLine(sql);
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
