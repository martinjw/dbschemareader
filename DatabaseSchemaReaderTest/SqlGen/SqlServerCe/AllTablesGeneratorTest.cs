using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServerCe
{
    [TestClass]
    public class AllTablesGeneratorTest
    {


        [TestMethod]
        public void TestSqlServerCeSchema()
        {
            //arrange
            var schema = new DatabaseSchema(null, SqlType.SqlServerCe);
            //two tables with a FK between them
            schema.AddTable("Categories")
                  .AddColumn<int>("Id").AddIdentity().AddPrimaryKey()
                  .AddColumn<string>("Name").AddLength(200)
                  .AddTable("Products")
                  .AddColumn<int>("Id").AddIdentity().AddPrimaryKey()
                  .AddColumn<string>("Name").AddLength(200)
                  .AddColumn<int>("CategoryId").AddForeignKey("Categories")
                  ;
            var factory = new DdlGeneratorFactory(SqlType.SqlServerCe);
            var tablesGenerator = factory.AllTablesGenerator(schema);
            tablesGenerator.IncludeSchema = false;

            //act
            var ddl = tablesGenerator.Write();

            //assert
            Assert.IsTrue(ddl.Contains("GO")); //batch separators
            Assert.IsFalse(ddl.Contains(";")); //valid but useless in SqlServer CE because you can't batch
        }



    }
}
