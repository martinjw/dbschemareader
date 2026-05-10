using DatabaseSchemaReader;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class ViewExistsTests
    {
        [TestMethod, TestCategory("SqlServer")]
        public void ViewExists()
        {
            var connectionString = ConnectionStrings.Northwind;
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var northwindReader = new DatabaseReader(con);
                    northwindReader.Owner = "dbo";
                    var views = northwindReader.AllViews();
                    foreach (var view in views)
                    {
                        var result = northwindReader.ViewExists(view.Name);
                        //assert
                        Assert.IsTrue(result, $"View {view.Name} should exist in Northwind database");
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Could not open Northwind: {e}");
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ViewDoesNotExist()
        {
            var connectionString = ConnectionStrings.Northwind;
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var northwindReader = new DatabaseReader(con);
                    northwindReader.Owner = "dbo";
                    var result = northwindReader.ViewExists("Does_Not_Exist");
                    //assert
                    Assert.IsFalse(result, "Does_Not_Exist view should not exist in Northwind database");
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Could not open Northwind: {e}");
            }
        }
    }
}