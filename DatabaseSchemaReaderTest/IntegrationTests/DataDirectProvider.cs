using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// The following databases should exist on localhost:
    ///     SqlServer with Northwind (integrated security) NOT
    ///         NB SQLExpress is not supported by this driver
    ///     Oracle Express with HR (userId HR, password HR)
    /// </summary>
    [TestClass]
    public class DataDirectProvider
    {
        [TestMethod, TestCategory("DataDirect.SqlServer")]
        public void DataDirectSqlServer()
        {
            //not sql express
            const string providername = "DDTek.SQLServer";
            const string connectionString = @"Server=localhost;AuthenticationMethod=NTLM;DatabaseName=AdventureWorks";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var product = schema.FindTableByName("Product");
            Assert.AreEqual(25, product.Columns.Count);

            var table = dbReader.Table("Product");
            Assert.AreEqual(25, table.Columns.Count);
        }

        [TestMethod, TestCategory("DataDirect.Oracle")]
        public void DataDirectOracle()
        {
            const string providername = "DDTek.Oracle";
            const string connectionString = "Host=localhost;Service Name=XE;User Id=HR;Password=HR;";
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
