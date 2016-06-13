using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServerCe;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServerCe
{
    [TestClass]
    public class TableGeneratorTest
    {


        [TestMethod]
        public void TestSqlServerCeTableWithIdentity()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServerCe);
            var table = schema.AddTable("Test")
                  .AddColumn<int>("Id").AddIdentity().AddPrimaryKey()
                  .AddColumn<string>("Name").AddLength(200)
                  .Table;
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("GO")); //batch separators
            Assert.IsFalse(ddl.Contains(";")); //valid but useless in SqlServer CE because you can't batch
        }



    }
}
