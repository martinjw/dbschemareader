using DatabaseSchemaReader;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class SqlLite
    {

        [TestMethod, TestCategory("SQLite")]
        public void SqlLiteTest()
        {
            var filePath = ConnectionStrings.SqLiteFilePath;
            if (!File.Exists(filePath))
            {
                Assert.Inconclusive("SqlLite test requires database file " + filePath);
            }
            var csb = new SqliteConnectionStringBuilder { DataSource = filePath };
            var connectionString = csb.ConnectionString;
            using (var con = new SqliteConnection(connectionString))
            {
                con.Open();
                var dbReader = new DatabaseReader(con);
                var schema = dbReader.ReadAll();
                var orders = schema.FindTableByName("Orders");
                Assert.IsTrue(orders.Columns.Count > 2); //we don't care if it's not standard Northwind
                var compoundKeys = schema.FindTableByName("CompoundKeys");
                Assert.IsTrue(compoundKeys.FindColumn("Key1").IsPrimaryKey);
                Assert.IsTrue(compoundKeys.FindColumn("Key2").IsPrimaryKey);
            }


        }
    }
}
