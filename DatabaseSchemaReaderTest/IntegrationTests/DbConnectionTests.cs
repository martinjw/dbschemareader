﻿using System;
using System.Data.SqlClient;
using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class DbConnectionTests
    {
        [TestMethod]
        public void TestSqlConnectionConstructor()
        {
            //also used by NetStandard
            using (var con = new SqlConnection(ConnectionStrings.Northwind))
            {
                var dr = new DatabaseReader(con);

                var tables = dr.AllTables();

                Assert.IsTrue(tables.Count > 0);
            }
        }

        [TestMethod]
        public void TestSqlConnectionConstructorWithNoConnectionString()
        {
            //also used by NetStandard
            using (var con = new SqlConnection())
            {
                var dr = new DatabaseReader(con);

                try
                {
                    dr.AllTables();
                    Assert.Fail("Should have errored");
                }
                catch (InvalidOperationException)
                {
                    Assert.IsTrue(true, "No connection string");
                }
            }
        }

        [TestMethod]
        public void RunSqlite()
        {
            using (var connection = new System.Data.SQLite.SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE Test(Value);";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO Test VALUES(1);";
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT [type], [name], [tbl_name], [rootpage], [sql], [rowid] FROM [main].[sqlite_master] WHERE [type] LIKE 'table'";
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            System.Console.WriteLine(dr[2].ToString());
                        }
                    }
                }

                var databaseReader = new DatabaseSchemaReader.DatabaseReader(connection);
                var tableList = databaseReader.TableList();
                Assert.IsTrue(tableList.Count > 0);
            }
        }
    }
}
