using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.Oracle;
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

namespace DatabaseSchemaReaderTest.SqlGen.Oracle
{
    [TestClass]
    public class OracleAutoNumberTest
    {
        [TestMethod]
        public void TestOracleTableWithTrigger()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.Oracle);
            var table = schema.AddTable("Test");
            var id = table.AddColumn<int>("Id").AddPrimaryKey();
            id.IsAutoNumber = true;
            table.AddColumn<string>("Name").AddLength(200);
            table.Triggers.Add(new DatabaseTrigger
                               {
                                   Name = "Test_INS_TRG",
                                   TriggerEvent = "INSERT",
                                   TriggerBody = @"BEGIN
  SELECT ""Test_SEQ"".NEXTVAL INTO :NEW.""Id"" FROM DUAL;
END;",
                                   TriggerType = "BEFORE EACH ROW",
                               });
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("\"Id\" NUMBER (9) NOT NULL,"));
            Assert.IsTrue(ddl.Contains(@"CREATE OR REPLACE TRIGGER ""Test_INS_TRG""
BEFORE INSERT
ON ""Test""
FOR EACH ROW
BEGIN
  SELECT ""Test_SEQ"".NEXTVAL INTO :NEW.""Id"" FROM DUAL;
END;"));
        }

        [TestMethod]
        public void TestSqlServerConversionTableWithIdentity()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test")
                .AddColumn<int>("Id").AddPrimaryKey().AddIdentity()
                .AddColumn<string>("Name").AddLength(200)
                .Table;
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("\"Id\" NUMBER (9) NOT NULL,"));
            Assert.IsTrue(ddl.Contains(@"CREATE OR REPLACE TRIGGER "), "Creates a trigger");
        }

        [TestMethod]
        public void TestOracle12TableWithSequenceAutoNumber()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.Oracle);
            var table = schema.AddTable("Test");
            var id = table.AddColumn<int>("Id").AddPrimaryKey();
            id.DefaultValue = "Seq.NEXTVAL";
            id.IsAutoNumber = true; //but id.IdentityDefinition == null
            table.AddColumn<string>("Name").AddLength(200);
            var tableGen = new TableGenerator(table);

            //acts
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("\"Id\" NUMBER (9) NOT NULL DEFAULT Seq.NEXTVAL"));
            Assert.IsFalse(ddl.Contains(@"CREATE OR REPLACE TRIGGER "));
        }

        [TestMethod]
        public void TestOracle12TableWithIdentityAutoNumber()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.Oracle);
            var table = schema.AddTable("Test")
                .AddColumn<int>("Id").AddPrimaryKey().AddIdentity()
                .AddColumn<string>("Name").AddLength(200)
                .Table;
            var tableGen = new TableGenerator(table);

            //acts
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("\"Id\" NUMBER (9) NOT NULL GENERATED AS IDENTITY"));
            Assert.IsFalse(ddl.Contains(@"CREATE OR REPLACE TRIGGER "));
        }
    }
}
