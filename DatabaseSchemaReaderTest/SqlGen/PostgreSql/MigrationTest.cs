using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.PostgreSql
{
    [TestClass]
    public class MigrationTest
    {
        [TestMethod, TestCategory("Postgresql")]
        public void TestDropTrigger()
        {
            //#197 PostgreSQL: Invalid DROP TRIGGER syntax (not sql standard!)
            var trigger = new DatabaseTrigger
            {
                Name = "TriggerName",
                SchemaOwner = "public",
                TableName = "Table1",
            };

            var factory = new DdlGeneratorFactory(SqlType.PostgreSql);
            var migrator = factory.MigrationGenerator();
            var sql = migrator.DropTrigger(trigger);
            Assert.AreEqual("DROP TRIGGER IF EXISTS \"TriggerName\" ON \"public\".\"Table1\";",sql);
        }

        [TestMethod, TestCategory("Postgresql")]
        public void TestAddTrigger()
        {
            //#198 PostgreSQL: DbSchemaReader adds extra semicolon at the end of the TriggerBody
            var table = new DatabaseTable
            {
                Name = "Table1",
                SchemaOwner = "public"
            };

            var trigger = new DatabaseTrigger
            {
                Name = "TriggerName",
                SchemaOwner = "public",
                TableName = table.Name,
                TriggerEvent = "UPDATE",
                TriggerBody = "EXECUTE FUNCTION last_updated()",
                TriggerType = "BEFORE"
            };

            var factory = new DdlGeneratorFactory(SqlType.PostgreSql);
            var migrator = factory.MigrationGenerator();
            var sql = migrator.AddTrigger(table, trigger);
            Assert.AreEqual("CREATE TRIGGER TriggerName BEFORE UPDATE ON \"public\".\"Table1\" FOR EACH ROW EXECUTE FUNCTION last_updated();", sql);
            //CREATE TRIGGER check_update
            //    BEFORE UPDATE ON accounts
            //    FOR EACH ROW
            //    EXECUTE FUNCTION last_updated();


        }
    }
}
