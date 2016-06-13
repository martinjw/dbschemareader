using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class Ingres
    {
        [TestMethod, TestCategory("Ingres")]
        public void IngresTest()
        {
            const string providername = "Ingres.Client";
            //you may need to add User Id=x;pwd=p (windows account)
            const string connectionString = "Host=localhost;database=demodb;";

            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();

            Assert.IsTrue(schema.Tables.Count > 0);
        }
    }
}
