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
    /// The following databases should exist on localhost:
    ///     Db2 with Sample (user id root, passwod mysql)
    /// </summary>
    [TestClass]
    public class Db2
    {
        [TestMethod, TestCategory("DB2")]
        public void Db2Test()
        {
            const string providername = "IBM.Data.DB2";
            const string connectionString = @"Server=localhost:50000;UID=db2admin;pwd=db2;Database=Sample";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var table = schema.FindTableByName("EMPLOYEE");
            Assert.IsTrue(table.Columns.Count > 0);
        }
    }
}
