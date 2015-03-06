using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
using TestCategory = NUnit.Framework.CategoryAttribute;
#endif

namespace DatabaseSchemaReaderTest.SqlGen.Migrations
{
    [TestClass]
    public class MigrationSqLiteTest
    {
        private const string ProviderName = "System.Data.SQLite";
        private const string DatabaseFile = @"C:\Data\northwind.db";
        private const string ConnectionString = "Data Source=" + DatabaseFile;

        [TestMethod, TestCategory("SQLite")]
        public void TestMigration()
        {
            //arrange
            var tableName = MigrationCommon.FindFreeTableName(ProviderName, ConnectionString);
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            MigrationCommon.ExecuteScripts(ProviderName, ConnectionString, tableName, migration);
        }
    }
}
