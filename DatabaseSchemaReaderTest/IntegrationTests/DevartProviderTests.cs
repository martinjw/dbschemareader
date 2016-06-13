using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseSchemaReader;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// The following databases should exist on localhost:
    ///     SqlExpress with Northwind (integrated security)
    ///     Oracle Express with HR (userId HR, password HR)
    /// </summary>
    [TestClass]
    public class DevartProviderTests
    {
        [TestMethod, TestCategory("Devart.Oracle")]
        public void DevartOracle()
        {
            const string providername = "Devart.Data.Oracle";
            const string connectionString = "Server=localhost;Sid=XE;Port=1521;Direct=true;User Id=hr;Password=hr;";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            dbReader.Owner = "HR";
            var schema = dbReader.ReadAll();
            var employees = schema.FindTableByName("EMPLOYEES");
            Assert.AreEqual(11, employees.Columns.Count);

            var table = dbReader.Table("EMPLOYEES");
            Assert.AreEqual(11, table.Columns.Count);
        }

        //ignored because it isn't installed on my machine.
        [TestMethod, TestCategory("Devart.SqlServer")]
        public void DevartSqlServer()
        {
            const string providername = "Devart.Data.SqlServer";
            const string connectionString = @"Data Source=localhost\SQLEXPRESS;Integrated Security=true;Initial Catalog=AdventureWorks";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var product = schema.FindTableByName("Product");
            Assert.AreEqual(25, product.Columns.Count);

            var table = dbReader.Table("Product");
            Assert.AreEqual(25, table.Columns.Count);
        }
    }
}
