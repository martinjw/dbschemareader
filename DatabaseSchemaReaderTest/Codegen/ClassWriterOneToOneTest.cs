using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class ClassWriterOneToOneTest
    {
        //One to one relationships are used for two main reasons:
        // * Divide a large table into smaller parts (Product - ProductDetail)
        // * Divide a table for security (or because it's updated frequently...) (User, UserPassword) 


        private static DatabaseSchema Arrange()
        {
            var schema = new DatabaseSchema(null, null);
            schema.AddTable("Products")
                  .AddColumn<int>("Id").AddPrimaryKey()
                  .AddColumn<string>("Name")

                  .AddTable("ProductDetails")
                  .AddColumn<int>("Id").AddPrimaryKey().AddForeignKey("Products")
                  .AddColumn<string>("Description");

            DatabaseSchemaFixer.UpdateDataTypes(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());

            return schema;
        }

        [TestMethod]
        public void TestOneToOneTable()
        {
            //arrange
            var schema = Arrange();
            var table = schema.FindTableByName("Products");
            var codeWriterSettings = new CodeWriterSettings {CodeTarget = CodeTarget.PocoEntityCodeFirst};
            var target = new CodeFirstMappingWriter(table, codeWriterSettings, new MappingNamer());

            //act
            var txt = target.Write();

            //assert
            Assert.IsTrue(txt.Contains("HasOptional(x => x.ProductDetail);"));
        }

    }
}
