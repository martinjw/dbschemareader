using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServer;
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

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    [TestClass]
    public class TableGeneratorTest
    {
        [TestMethod]
        public void TestSqlServerTableWithComputedColumn()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("AllTypes")
                  .AddColumn<int>("Id").AddIdentity()
                  .AddColumn<string>("Name").AddLength(200)
                  .AddColumn<int>("Age")
                  .AddColumn<int>("Period")
                  .Table;
            table.AddColumn<int>("ComputedAge").ComputedDefinition = "(Age - Period)";
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("[ComputedAge] AS (Age - Period)"));
        }

        [TestMethod]
        public void TestSqlServerTableWithIdentity()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test")
                  .AddColumn<int>("Id").AddIdentity()
                  .AddColumn<string>("Name").AddLength(200)
                  .Table;
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("[Id] INT IDENTITY(1,1)  NOT NULL"));
        }

        [TestMethod]
        public void TestSqlServerTableWithSequenceAutoNumber()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test");
            var id = table.AddColumn<int>("Id").AddPrimaryKey();
            id.DefaultValue = "NEXT VALUE FOR [MySequence]";
            id.IsAutoNumber = true; //but id.IdentityDefinition == null
            table.AddColumn<string>("Name").AddLength(200);
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("[Id] INT NOT NULL DEFAULT NEXT VALUE FOR [MySequence]"));
        }

    }
}
