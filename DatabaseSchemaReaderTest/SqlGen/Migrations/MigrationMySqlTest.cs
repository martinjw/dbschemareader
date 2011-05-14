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
    public class MigrationMySqlTest
    {
        private const string ProviderName = "MySql.Data.MySqlClient";
        private const string ConnectionString = @"Server=localhost;Uid=root;Pwd=mysql;Database=Northwind;Allow User Variables=True;";


        [TestMethod]
        public void TestMigration()
        {
            //arrange
            var tableName = MigrationCommon.FindFreeTableName(ProviderName, ConnectionString);
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            //MySql DDL isn't transactional. Hope this works.
            MigrationCommon.ExecuteScripts(ProviderName, ConnectionString, tableName, migration);
        }
    }
}
