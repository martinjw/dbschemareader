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
    ///     MySQL with Northwind (user id root, passwod mysql)
    /// </summary>
    [TestClass]
    public class MySql
    {
        [TestMethod]
        public void MySqlTest()
        {
            const string providername = "MySql.Data.MySqlClient";
            const string connectionString = @"Server=localhost;Uid=root;Pwd=mysql;Database=Northwind;Allow User Variables=True;";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var orders = schema.FindTableByName("Orders");
            Assert.AreEqual(14, orders.Columns.Count);

            var table = dbReader.Table("Orders");
            Assert.AreEqual(14, table.Columns.Count);
        }
    }
}
