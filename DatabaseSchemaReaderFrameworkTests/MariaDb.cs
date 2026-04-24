using DatabaseSchemaReader;

namespace DatabaseSchemaReaderFrameworkTests
{
    [TestClass]
    public class MariaDb
    {
        [TestMethod, TestCategory("MariaDb")]
        public void MariaDbTest()
        {
            var connectionString = "Server=127.0.0.1;User ID=root;Password=Secret;Port=3308;Database=nation";
            using (var connection = new MySqlConnector.MySqlConnection(connectionString))
            {
                ProviderChecker.Check(connection);
                var dbReader = new DatabaseReader(connection);
                var schema = dbReader.ReadAll();
                Assert.IsTrue(schema.Tables.Count > 0);
            }
        }
    }
}
