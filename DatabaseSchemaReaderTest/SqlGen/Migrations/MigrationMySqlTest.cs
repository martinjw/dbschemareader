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
    public class MigrationMySqlTest
    {
        private const string ProviderName = "MySql.Data.MySqlClient";


        [TestMethod, TestCategory("MySql")]
        public void TestMigration()
        {
            //arrange
            var connectionString = ConnectionStrings.MySql;
            var tableName = MigrationCommon.FindFreeTableName(ProviderName, connectionString);
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            //MySql DDL isn't transactional. Hope this works.
            MigrationCommon.ExecuteScripts(ProviderName, connectionString, tableName, migration);
        }
    }
}
