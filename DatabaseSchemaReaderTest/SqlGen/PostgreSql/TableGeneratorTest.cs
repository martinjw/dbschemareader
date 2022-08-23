using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.PostgreSql
{
    [TestClass]
    public class TableGeneratorTest
    {
        [TestMethod]
        public void TestPostgreSqlTableWithDefaultValue()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.PostgreSql);
            var table = schema.AddTable("AllTypes")
                .AddColumn<int>("Id").AddIdentity()
                .AddColumn<string>("Name").AddLength(200)
                .AddColumn<int>("Age")
                .AddColumn<int>("Period")
                .Table;
            table.AddColumn<int>("IsBig").AddNullable().DefaultValue = "1";
            var tableGen = new TableGenerator(table);

            //act
            var ddl = tableGen.Write();

            //assert
            Assert.IsTrue(ddl.Contains("\"IsBig\" INTEGER  DEFAULT 1"));
        }
    }
}