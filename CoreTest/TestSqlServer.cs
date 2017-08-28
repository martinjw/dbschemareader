using System;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTest
{
    [TestClass]
    public class TestSqlServer
    {
        public static string Northwind
        {
            get
            {
                if (string.Equals("True", Environment.GetEnvironmentVariable("APPVEYOR")))
                {
                    return @"Server=(local)\SQL2016;Database=NorthwindDsr;User ID=sa;Password=Password12!";
                }
                return @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
            }
        }

        [TestMethod]
        public void RunTableList()
        {
            using (var connection = new SqlConnection(Northwind))
            {
                var dr = new DatabaseSchemaReader.DatabaseReader(connection);
                var schema = dr.ReadAll();
                var tableList = dr.TableList();
                var tables = dr.AllTables();
                var views = dr.AllViews();
                Assert.IsTrue(tableList.Count > 0);
            }
        }

        [TestMethod]
        public void RunTableListWithTransaction()
        {
            using (var connection = new SqlConnection(Northwind))
            {
                connection.Open();
                using (var txn = connection.BeginTransaction())
                {
                    var dr = new DatabaseSchemaReader.DatabaseReader(txn);
                    var schema = dr.ReadAll();
                    var tableList = dr.TableList();
                    var tables = dr.AllTables();
                    var views = dr.AllViews();
                    Assert.IsTrue(tableList.Count > 0);

                    txn.Rollback();
                }
            }
        }
    }
}