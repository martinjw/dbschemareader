using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations
{
    [TestClass]
    public class MigrationOracleTest
    {
        private const string ProviderName = "System.Data.OracleClient";

        [TestMethod, TestCategory("Oracle")]
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
