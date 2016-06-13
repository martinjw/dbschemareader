using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class OracleSequenceTriggerTest
    {
        [TestMethod]
        public void FindOracleAutoNumberTrigger()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.Oracle);
            var table = schema.AddTable("Test");
            var id = table.AddColumn<int>("Id").AddPrimaryKey();
            id.IsAutoNumber = true;
            table.AddColumn<string>("Name").AddLength(200);
            var databaseTrigger = new DatabaseTrigger
                                  {
                                      Name = "Test_INS_TRG",
                                      TriggerEvent = "INSERT",
                                      TriggerBody = @"BEGIN
  SELECT ""Test_SEQ"".NEXTVAL INTO :NEW.""Id"" FROM DUAL;
END;",
                                      TriggerType = "BEFORE EACH ROW",
                                  };
            table.Triggers.Add(databaseTrigger);
            var databaseSequence = new DatabaseSequence { IncrementBy = 1, MinimumValue = 0, Name = "Test_SEQ" };
            schema.Sequences.Add(databaseSequence);

            //act
            var result = OracleSequenceTrigger.FindTrigger(table);

            //assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.DatabaseTrigger);
            Assert.IsNotNull(result.DatabaseSequence);
            Assert.AreEqual(databaseSequence, result.DatabaseSequence);
            Assert.AreEqual(databaseTrigger, result.DatabaseTrigger);
        }
    }
}
