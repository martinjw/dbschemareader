using System.IO;
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
    /// </summary>
    [TestClass]
    public class SqlServerCe4
    {
        //MSDN is here: http://msdn.microsoft.com/en-us/library/ff929050%28v=SQL.10%29.aspx

        private const string ProviderName = "System.Data.SqlServerCe.4.0";
        private const string FilePath = @"C:\Data\northwind.sdf";

        [TestMethod]
        public void SqlServerCe4Test()
        {
            if (!File.Exists(FilePath))
            {
                Assert.Inconclusive("SqlServerCe4 test requires database file " + FilePath);
            }

            const string connectionString = "Data Source=\"" + FilePath + "\"";
            ProviderChecker.Check(ProviderName, connectionString);

            var dbReader = new DatabaseReader(connectionString, ProviderName);
            var schema = dbReader.ReadAll();
            var orders = schema.FindTableByName("Orders");
            Assert.IsTrue(orders.Columns.Count > 2); //we don't care if it's not standard Northwind

        }
    }
}
