using System.Data;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{

    /// <summary>
    /// Test code first DbContext
    ///</summary>
    [TestClass]
    public class CodeFirstContextWriterTest
    {

        /// <summary>
        ///A test for Execute with CodeFirst
        ///</summary>
        [TestMethod]
        public void ExecuteTest()
        {
            //arrange
            DatabaseSchema schema = PrepareModel();
            var settings = new CodeWriterSettings();
            var target = new CodeFirstContextWriter(settings);

            //act
            var result = target.Write(schema.Tables);

            //assert
            var hasDbSet = result.Contains("public IDbSet<Product> ProductCollection");
            var hasMapping = result.Contains("modelBuilder.Configurations.Add(new ProductMapping());");
            Assert.IsTrue(hasDbSet);
            Assert.IsTrue(hasMapping);
        }

        private static DatabaseSchema PrepareModel()
        {
            var schema = new DatabaseSchema(null, null);

            schema.AddTable("Categories")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddColumn("CategoryName", DbType.String);

            schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey()
                .AddColumn("ProductName", DbType.String);

            schema.AddTable("CategoryProducts")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddForeignKey("fk", "Categories")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey()
                .AddForeignKey("fk", "Products");

            DatabaseSchemaFixer.UpdateReferences(schema);
            PrepareSchemaNames.Prepare(schema, new Namer());

            return schema;
        }
    }
}
