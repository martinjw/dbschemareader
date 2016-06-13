using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class ClassWriterAssociationTableTest
    {
        [TestMethod]
        public void TestAssociationTable()
        {
            //arrange
            var schema = Arrange();
            var table = schema.FindTableByName("ProductCategories");
            var cw = new ClassWriter(table, new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst });

            //act
            var txt = cw.Write();

            //assert
            var hasCategoryId = txt.Contains("public int CategoryId { get; set; }");
            var hasCategory = txt.Contains("public virtual Category Category { get; set; }");
            var hasProductId = txt.Contains("public int ProductId { get; set; }");
            var hasProduct = txt.Contains("public virtual Product Product { get; set; }");
            Assert.IsTrue(hasCategoryId);
            Assert.IsTrue(hasCategory);
            Assert.IsTrue(hasProductId);
            Assert.IsTrue(hasProduct);
        }

        private static DatabaseSchema Arrange()
        {
            var schema = new DatabaseSchema(null, null);
            schema.AddTable("Categories")
                  .AddColumn<int>("CategoryId").AddPrimaryKey()
                  .AddColumn<string>("CategoryName")

                  .AddTable("Products")
                  .AddColumn<int>("ProductId").AddPrimaryKey()
                  .AddColumn<string>("ProductName");

            var assoc =
                schema.AddTable("ProductCategories")
                  .AddColumn<int>("CategoryId").AddPrimaryKey().AddForeignKey("Categories")
                  .AddColumn<int>("ProductId").AddForeignKey("Products")
                  .Table;

            assoc.PrimaryKey.AddColumn(assoc.FindColumn("ProductId"));

            DatabaseSchemaFixer.UpdateDataTypes(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());

            return schema;
        }
    }
}
