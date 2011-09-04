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
    [TestClass]
    public class Access
    {
        //does not detect autonumber pks

        [TestMethod]
        public void TestAccess97()
        {
            const string providername = "System.Data.OleDb";
            const string dir = @"C:\Data\Nwind.mdb";
            if (!File.Exists(dir))
            {
                Assert.Inconclusive("Access test requires database file " + dir);
            }

            const string connectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + dir;
            ProviderChecker.Check(providername, connectionString);

            DatabaseSchemaReader.Utilities.DiscoverProviderFactory.Discover(connectionString, providername);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var table = schema.FindTableByName("Products");

            Assert.IsTrue(table.Columns.Count > 0);
        }

        [TestMethod]
        public void TestAccess2007()
        {
            const string providername = "System.Data.OleDb";
            const string dir = @"C:\Data\Nwind.accdb";
            if (!File.Exists(dir))
            {
                Assert.Inconclusive("Access test requires database file " + dir);
            }

            const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + dir;
            ProviderChecker.Check(providername, connectionString);

            DatabaseSchemaReader.Utilities.DiscoverProviderFactory.Discover(connectionString, providername);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var table = schema.FindTableByName("Products");

            Assert.IsTrue(table.Columns.Count > 0);
        }
    }
}
