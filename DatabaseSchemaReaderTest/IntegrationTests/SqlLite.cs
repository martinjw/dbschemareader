using System.IO;
using DatabaseSchemaReader;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
using TestCategory = NUnit.Framework.CategoryAttribute;
#endif

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class SqlLite
    {

        [TestMethod, TestCategory("SQLite")]
        public void SqlLiteTest()
        {
            const string providername = "System.Data.SQLite";
            const string dir = @"C:\Data\northwind.db";
            if (!File.Exists(dir))
            {
                Assert.Inconclusive("SqlLite test requires database file " + dir);
            }

            const string connectionString = @"Data Source=" + dir;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var orders = schema.FindTableByName("Orders");
            Assert.IsTrue(orders.Columns.Count > 2); //we don't care if it's not standard Northwind
        }
    }
}
