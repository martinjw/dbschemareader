using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// The following databases should exist on localhost:
    ///     Db2 with Sample (user id db2, passwod db2)
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

        [TestMethod, TestCategory("DB2")]
        public void DiscoverDb2Schemas()
        {
            const string providername = "IBM.Data.DB2";
            const string connectionString = @"Server=localhost:50000;UID=db2admin;pwd=db2;Database=Sample";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schemas = dbReader.AllSchemas();

            Assert.IsTrue(schemas.Count > 0);
        }
    }
}
