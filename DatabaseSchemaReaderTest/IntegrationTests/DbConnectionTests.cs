using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using Microsoft.Data.SqlClient;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class DbConnectionTests
    {
        private static bool CheckNorthwindExists(SqlConnection con)
        {
            try
            {
                con.Open();
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
                return false;
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void TestSqlConnectionConstructor()
        {
            //also used by NetStandard
            using (var con = new SqlConnection(ConnectionStrings.Northwind))
            {
                if (!CheckNorthwindExists(con))
                {
                    Assert.Inconclusive($"Could not access Northwind {con.ConnectionString}");
                    return;
                }
                var dr = new DatabaseReader(con);

                var tables = dr.AllTables();

                Assert.IsTrue(tables.Count > 0);
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void TestSqlConnectionConstructorWithNoConnectionString()
        {
            //also used by NetStandard
            using (var con = new SqlConnection())
            {
                if (!CheckNorthwindExists(con))
                {
                    Assert.Inconclusive($"Could not access Northwind {con.ConnectionString}");
                    return;
                }
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
    }
}
