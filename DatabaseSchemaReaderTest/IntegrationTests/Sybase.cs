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
    public class Sybase
    {
        [TestMethod]
        public void SybaseAnyWhereTest()
        {
            const string providername = "iAnyWhere.Data.SQLAnyWhere";
            const string connectionString = "Data Source=SQL Anywhere 12 Demo";

            ProviderChecker.Check(providername, connectionString);

            DatabaseSchemaReader.Utilities.DiscoverProviderFactory.Discover(connectionString, providername);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();

            Assert.IsTrue(schema.Tables.Count > 0);
        }
    }
}
