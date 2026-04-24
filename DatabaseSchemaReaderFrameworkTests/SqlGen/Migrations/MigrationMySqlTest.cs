using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReaderFrameworkTests.Utilities;

namespace DatabaseSchemaReaderFrameworkTests.SqlGen.Migrations
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
            var factory = new MySqlConnectorSetup().EnsureProviderFactory();
            var tableName = MigrationCommon.FindFreeTableName(factory, connectionString);
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            //MySql DDL isn't transactional. Hope this works.
            MigrationCommon.ExecuteScripts(factory, connectionString, tableName, migration);
        }
    }
}
