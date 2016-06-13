using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations
{
    [TestClass]
    public class MigrationSqLiteTest
    {
        private const string ProviderName = "System.Data.SQLite";

        [TestMethod, TestCategory("SQLite")]
        public void TestMigration()
        {
            var databaseFile = ConnectionStrings.SqLiteFilePath;
            var connectionString = "Data Source=" + databaseFile;
            //arrange
            var tableName = MigrationCommon.FindFreeTableName(ProviderName, connectionString);
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            MigrationCommon.ExecuteScripts(ProviderName, connectionString, tableName, migration);
        }
    }
}
