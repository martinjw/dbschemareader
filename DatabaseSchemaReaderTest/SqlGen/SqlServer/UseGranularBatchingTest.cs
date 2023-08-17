using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    [TestClass]
    public class UseGranularBatchingTest
    {
        [TestMethod]
        public void TestGranularBatching()
        {
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            schema.AddTable("Test")
                .AddColumn<int>("Id").AddIdentity()
                .AddColumn<string>("Name").AddLength(200)
                .AddTable("Test2")
                .AddColumn<int>("Id").AddIdentity()
                .AddColumn<string>("Name").AddLength(300)
                .AddColumn<int>("TestId")
                .AddForeignKey("Test2FK", "Test");

            var ddlGeneratorFactory = new DdlGeneratorFactory(SqlType.SqlServer);
            var ddl = ddlGeneratorFactory.AllTablesGenerator(schema).Write();

            ddlGeneratorFactory.UseGranularBatching = true;
            var dllWithBatching = ddlGeneratorFactory.AllTablesGenerator(schema).Write();

            Assert.IsFalse(ddl.Contains("GO"));
            Assert.IsTrue(dllWithBatching.Contains("GO"));
        }
    }
}
