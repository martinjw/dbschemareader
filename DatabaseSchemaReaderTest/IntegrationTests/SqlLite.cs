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
    /// </summary>
    [TestClass]
    public class SqlLite
    {

        //[TestMethod]
        //public void SqlLiteTest()
        //{
        //    var providers = SchemaExtendedReader.Providers();
        //    var dir = @"D:\Data\northwind.db";
        //    if (!File.Exists(dir)) return;

        //    const string providername = "System.Data.SQLite";
        //    const string connectionString = @"Data Source=" + dir;

        //    var dbReader = new DatabaseReader(connectionString, providername);
        //    var schema = dbReader.ReadAll();
        //    var Orders = schema.FindTableByName("Orders");
        //    Assert.AreEqual(13, Orders.Columns.Count);

        //    var table = dbReader.Table("Orders");
        //    Assert.AreEqual(13, table.Columns.Count);
        //}
    }
}
