using System.IO;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Utilities.DbProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class Access
    {
        //does not detect autonumber pks

        [TestMethod, TestCategory("Access")]
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

            DiscoverProviderFactory.Discover(connectionString, providername);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var table = schema.FindTableByName("Products");

            Assert.IsTrue(table.Columns.Count > 0);
        }

        [TestMethod, TestCategory("Access")]
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

            DiscoverProviderFactory.Discover(connectionString, providername);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var table = schema.FindTableByName("Products");

            Assert.IsTrue(table.Columns.Count > 0);
        }
    }
}
