using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations
{
    [TestClass]
    public class MigrationDb2Test
    {
        const string ProviderName = "IBM.Data.DB2";
        const string ConnectionString = @"Server=localhost:50000;UID=db2admin;pwd=db2;Database=Sample";

        [TestMethod, TestCategory("DB2")]
        public void TestMigration()
        {

            //arrange
            var tableName = MigrationCommon.FindFreeTableName(ProviderName, ConnectionString);
            var migration = new DdlGeneratorFactory(SqlType.Db2).MigrationGenerator();

            MigrationCommon.ExecuteScripts(ProviderName, ConnectionString, tableName, migration);
        }
    }
}
