using System;
using System.IO;
using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            const string providername = "System.Data.SQLite";
            var dir = ConnectionStrings.SqLiteFilePath;
            if (!File.Exists(dir))
            {
                Assert.Inconclusive("SqlLite test requires database file " + dir);
            }

            string connectionString = @"Data Source=" + dir;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var orders = schema.FindTableByName("Orders");
            Assert.IsTrue(orders.Columns.Count > 2); //we don't care if it's not standard Northwind
            var compoundKeys = schema.FindTableByName("CompoundKeys");
            Assert.IsTrue(compoundKeys.FindColumn("Key1").IsPrimaryKey);
            Assert.IsTrue(compoundKeys.FindColumn("Key2").IsPrimaryKey);
        }
    }
}
