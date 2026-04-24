using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

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
            var tableName = MigrationCommon.FindFreeTableName(ProviderName, connectionString);
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            //MySql DDL isn't transactional. Hope this works.
            MigrationCommon.ExecuteScripts(ProviderName, connectionString, tableName, migration);
        }
    }
}
