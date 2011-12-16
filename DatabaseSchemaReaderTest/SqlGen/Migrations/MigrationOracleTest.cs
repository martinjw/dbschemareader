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
#endif

namespace DatabaseSchemaReaderTest.SqlGen.Migrations
{
    [TestClass]
    public class MigrationOracleTest
    {
        private const string ProviderName = "System.Data.OracleClient";

        [TestMethod]
        public void TestMigration()
        {
            //arrange
            const string connectionString = ConnectionStrings.OracleHr;

            var tableName = MigrationCommon.FindFreeTableName(ProviderName, connectionString);
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            //Oracle DDL isn't transactional. If it fails, you'll find my test tables in your database.
            MigrationCommon.ExecuteScripts(ProviderName, connectionString, tableName, migration);
        }
    }
}
