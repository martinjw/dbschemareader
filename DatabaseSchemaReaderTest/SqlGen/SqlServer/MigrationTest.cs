using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    [TestClass]
    public class MigrationTest
    {
        [TestMethod, TestCategory("SqlServer")]
        public void TestAddTrigger()
        {
            //#199
            var table = new DatabaseTable
            {
                Name = "Table1",
            };

            var trigger = new DatabaseTrigger
            {
                Name = "TriggerName",
                TableName = table.Name,
                TriggerEvent = "INSERT",
                TriggerBody = @"CREATE TRIGGER TriggerName
ON Table1
FOR INSERT AS
IF @@ROWCOUNT = 1
BEGIN
    UPDATE Table1
    SET DATE_UPDATED = CURRENT_TIMESTAMP
    FROM inserted
    WHERE Table1 = inserted.ID;
END
;",
            };

            var factory = new DdlGeneratorFactory(SqlType.SqlServer);
            var migrator = factory.MigrationGenerator();
            var sql = migrator.AddTrigger(table, trigger);
            Assert.IsTrue(sql.StartsWith("GO"));


        }

    }
}
