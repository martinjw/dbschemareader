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
#endif

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class SqlLite
    {

        [TestMethod]
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
            Assert.AreEqual(13, orders.Columns.Count);

            var table = dbReader.Table("Orders");
            Assert.AreEqual(13, table.Columns.Count);
        }
    }
}
