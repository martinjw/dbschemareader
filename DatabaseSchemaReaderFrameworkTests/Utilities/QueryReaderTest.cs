using DatabaseSchemaReader.Utilities;

namespace DatabaseSchemaReaderFrameworkTests.Utilities
{
    [TestClass]
    public class QueryReaderTest
    {
        [TestMethod]
        public void TestProductsQuery()
        {
            const string providername = "System.Data.SqlClient";
            var connectionString = ConnectionStrings.Northwind;
            ProviderChecker.Check(providername, connectionString);

            var reader = new QueryReader();
            var columns = reader.GetQueryColumns(connectionString, providername, "Select ProductID, ProductName FROM Products");

            Assert.AreEqual(2, columns.Count);
        }
    }
}
