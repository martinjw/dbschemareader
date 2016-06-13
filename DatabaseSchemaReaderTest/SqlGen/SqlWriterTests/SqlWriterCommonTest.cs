using System;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlWriterTests
{
    class SqlWriterCommonTest
    {
        private readonly SqlType _sqlType;
        private readonly DatabaseTable _table;
        private readonly DbProviderFactory _factory;
        private readonly string _connectionString;

        public SqlWriterCommonTest(SqlType sqlType, DatabaseTable table, DbProviderFactory factory, string connectionString)
        {
            _connectionString = connectionString;
            _factory = factory;
            _table = table;
            _sqlType = sqlType;
        }

        public void RunCountSql()
        {
            //arrange
            var writer = new SqlWriter(_table, _sqlType);
            var sql = writer.CountSql();
            int count;

            //run generated sql
            using (var con = _factory.CreateConnection())
            {
                con.ConnectionString = _connectionString;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    con.Open();
                    //this returns an int in SqlServer and MySQL, a long in SQLite and a decimal(!) in Oracle
                    count = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }

            //assert
            Assert.IsTrue(count > 0, "There should be some categories (this test may fail if database table is empty)");
        }

        public void RunSelectAllSql()
        {
            //arrange
            var writer = new SqlWriter(_table, _sqlType);
            var sql = writer.SelectAllSql();
            var dataTable = new DataTable();

            //run generated sql
            using (var con = _factory.CreateConnection())
            {
                con.ConnectionString = _connectionString;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var da = _factory.CreateDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dataTable);
                }
            }

            //assert
            Assert.IsTrue(dataTable.Rows.Count > 0, "There should be some data rows (this test may fail if database table is empty)");
            foreach (var column in _table.Columns)
            {
                var name = column.Name;
                Assert.IsTrue(dataTable.Columns.Contains(name), "Should retrieve column " + name);
            }
        }

        public void RunPagingSql()
        {
            //arrange
            var writer = new SqlWriter(_table, _sqlType);
            var sql = writer.SelectPageSql();
            var dataTable = new DataTable();
            Console.WriteLine(sql);

            //run generated sql
            using (var con = _factory.CreateConnection())
            {
                con.ConnectionString = _connectionString;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var pageSize = cmd.CreateParameter();
                    pageSize.ParameterName = writer.ParameterName("pageSize");
                    pageSize.Value = 4;
                    cmd.Parameters.Add(pageSize);

                    var currentPage = cmd.CreateParameter();
                    currentPage.ParameterName = writer.ParameterName("currentPage");
                    currentPage.Value = 1;
                    cmd.Parameters.Add(currentPage);


                    var da = _factory.CreateDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dataTable);
                }
            }

            //assert
            Assert.IsTrue(dataTable.Rows.Count > 0, "There should be some categories (this test may fail if database table is empty)");
            Assert.IsTrue(dataTable.Rows.Count <= 4, "Should only return the page size (or less)");
            foreach (var column in _table.Columns)
            {
                var name = column.Name;
                Assert.IsTrue(dataTable.Columns.Contains(name), "Should retrieve column " + name);
            }
        }

        public void RunPagingStartToEndSql()
        {
            //arrange
            var writer = new SqlWriter(_table, _sqlType);
            var sql = writer.SelectPageStartToEndRowSql();
            var dataTable = new DataTable();
            Console.WriteLine(sql);

            //run generated sql
            using (var con = _factory.CreateConnection())
            {
                con.ConnectionString = _connectionString;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = sql;
                    var pageSize = cmd.CreateParameter();
                    pageSize.ParameterName = writer.ParameterName("startRow");
                    pageSize.Value = 1;
                    cmd.Parameters.Add(pageSize);

                    var currentPage = cmd.CreateParameter();
                    currentPage.ParameterName = writer.ParameterName("endRow");
                    currentPage.Value = 4;
                    cmd.Parameters.Add(currentPage);


                    var da = _factory.CreateDataAdapter();
                    da.SelectCommand = cmd;
                    da.Fill(dataTable);
                }
            }

            //assert
            Assert.IsTrue(dataTable.Rows.Count > 0, "There should be some categories (this test may fail if database table is empty)");
            Assert.IsTrue(dataTable.Rows.Count <= 4, "Should only return the page size (or less)");
            foreach (var column in _table.Columns)
            {
                var name = column.Name;
                Assert.IsTrue(dataTable.Columns.Contains(name), "Should retrieve column " + name);
            }
        }
    }
}
