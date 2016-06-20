using System.IO;
using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class SqlServerCe4
    {
        //MSDN is here: http://msdn.microsoft.com/en-us/library/ff929050%28v=SQL.10%29.aspx

        private const string ProviderName = "System.Data.SqlServerCe.4.0";

        [TestMethod, TestCategory("SqlServerCe")]
        public void SqlServerCe4Test()
        {
            if (!File.Exists(ConnectionStrings.SqlServerCeFilePath))
            {
                Assert.Inconclusive("SqlServerCe4 test requires database file " + ConnectionStrings.SqlServerCeFilePath);
            }

            var connectionString = "Data Source=\"" + ConnectionStrings.SqlServerCeFilePath + "\"";
            ProviderChecker.Check(ProviderName, connectionString);

            var dbReader = new DatabaseReader(connectionString, ProviderName);
            var schema = dbReader.ReadAll();
            var orders = schema.FindTableByName("Orders");
            Assert.IsTrue(orders.Columns.Count > 2); //we don't care if it's not standard Northwind

        }
    }
}
