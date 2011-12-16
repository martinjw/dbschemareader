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
    /// The following databases should exist on localhost:
    ///     Oracle Express with HR (userId HR, password HR)
    /// </summary>
    [TestClass]
    public class OracleDataProvider
    {
        [TestMethod]
        public void OracleOdp()
        {
            const string providername = "Oracle.DataAccess.Client";
            const string connectionString = ConnectionStrings.OracleHr;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            dbReader.Owner = "HR";
            var schema = dbReader.ReadAll();
            var employees = schema.FindTableByName("EMPLOYEES");
            Assert.AreEqual(11, employees.Columns.Count);

            var table = dbReader.Table("EMPLOYEES");
            Assert.AreEqual(11, table.Columns.Count);
        }
    }
}
