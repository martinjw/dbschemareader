using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseSchemaReader.Procedures;

namespace CoreTest
{
    [TestClass]
    public class TestMicrosoftSqlClient
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
                //Microsoft.Data.SqlClient needs a longer timeout than System.Data.SqlClient (on the stored procedure query)
                dr.CommandTimeout = 60;
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
                    dr.CommandTimeout = 60;
                    var schema = dr.ReadAll();
                    var tableList = dr.TableList();
                    var tables = dr.AllTables();
                    var views = dr.AllViews();
                    Assert.IsTrue(tableList.Count > 0);

                    txn.Rollback();
                }
            }
        }

        [TestMethod]
        public void ReadResultSets()
        {
            using (var connection = new SqlConnection(Northwind))
            {
                connection.Open();
                var dr = new DatabaseSchemaReader.DatabaseReader(connection);
                dr.CommandTimeout = 60;
                dr.AllStoredProcedures();
                var schema = dr.DatabaseSchema;

                var rsr = new ResultSetReader(schema);
                rsr.Execute(connection);

                var sproc = schema.StoredProcedures.Find(x => x.Name == "SalesByCategory");
                Assert.IsNotNull(sproc);
                var rs = sproc.ResultSets.First();
                foreach (var rsColumn in rs.Columns)
                {
                    Console.WriteLine(rsColumn.Name);
                    Console.WriteLine(rsColumn.DbDataType);
                }
                Assert.IsNotNull(rs, "Stored procedure should return a result");

            }
        }

        [TestMethod]
        public void ReadData()
        {
            using (var connection = new SqlConnection(Northwind))
            {
                connection.Open();
                var dr = new DatabaseSchemaReader.DatabaseReader(connection);
                dr.CommandTimeout = 60;
                var table = dr.Table("Categories");

                var reader = new DatabaseSchemaReader.Data.Reader(table);
                var dt = reader.Read(connection);
                Assert.IsTrue(dt.Rows.Count > 0);
            }
        }

        [TestMethod]
        public void ReadDataWithFunc()
        {
            using (var connection = new SqlConnection(Northwind))
            {
                connection.Open();
                var dr = new DatabaseSchemaReader.DatabaseReader(connection);
                dr.CommandTimeout = 60;
                var table = dr.Table("Categories");

                var reader = new DatabaseSchemaReader.Data.Reader(table);

                var names = new List<string>();

                reader.Read(connection, dataRecord =>
                {
                    var name = dataRecord["CategoryName"].ToString();
                    names.Add(name);
                    return true;
                });
                Assert.IsTrue(names.Count > 0);
            }
        }
    }
}