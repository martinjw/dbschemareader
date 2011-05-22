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
    public class MigrationPostgreSqlTest
    {
        //private const string ProviderName = "Npgsql";
        private const string ProviderName = "Devart.Data.PostgreSql";
        private const string ConnectionString = @"Server=127.0.0.1;User id=postgres;password=sql;database=world;";

        /*
 
CREATE FUNCTION X_Insert(IN INT,IN VARCHAR(20),IN VARCHAR(30))
RETURNS VOID AS $$ 
INSERT INTO X(XId, XName, XAddress) VALUES($1, $2, $3); 
$$ LANGUAGE SQL;
 
ALTER TABLE products ALTER COLUMN price SET DEFAULT 7.77;
 */

        [TestMethod]
        public void TestMigration()
        {
            //arrange
            var tableName = MigrationCommon.FindFreeTableName(ProviderName, ConnectionString);
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            MigrationCommon.ExecuteScripts(ProviderName, ConnectionString, tableName, migration);
        }
    }
}
