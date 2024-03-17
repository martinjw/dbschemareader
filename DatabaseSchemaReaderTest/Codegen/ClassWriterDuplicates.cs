using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class ClassWriterDuplicates
    {
        // #180 c# code gen does not compile
        [TestMethod]
        public void TestForDuplicates()
        {
            //arrange
            var schema = PrepareModel();

            var categories = schema.FindTableByName("Categories");
            var cw = new ClassWriter(categories, new CodeWriterSettings());

            //act
            var txt = cw.Write();

            //assert
            Assert.IsTrue(txt.Contains("ProductCategoryCollection"));
            Assert.IsTrue(txt.Contains("LocationCategoryCollection"));
            //Product and Location have TWO fks to Category (say, category 1, category 2).
            //       public Category()
            //        {
            //            CategoryCollection = new List<Product>();
            //            CategoryId2Collection = new List<Product>();
            //            CategoryCollection = new List<Location>();
            //            CategoryId2Collection = new List<Location>();
            //        }
            //should be:
            //        public Category()
            //        {
            //            ProductCategoryCollection = new List<Product>();
            //            ProductCategoryId2Collection = new List<Product>();
            //            LocationCategoryCollection = new List<Location>();
            //            LocationCategoryId2Collection = new List<Location>();
            //        }
        }

        private static DatabaseSchema PrepareModel()
        {
            var schema = new DatabaseSchema(null, null);

            var categories = "Categories";

            schema.AddTable(categories)
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddColumn("CategoryName", DbType.String);

            schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey()
                .AddColumn("Name", DbType.String)
                .AddColumn("CategoryId", DbType.Int32).AddForeignKey("fk", categories)
                .AddColumn("CategoryId2", DbType.Int32).AddForeignKey("fk", categories);

            schema.AddTable("Locations")
                .AddColumn("LocationId", DbType.Int32).AddPrimaryKey()
                .AddColumn("Name", DbType.String)
                .AddColumn("CategoryId", DbType.Int32).AddForeignKey("fk", categories)
                .AddColumn("CategoryId2", DbType.Int32).AddForeignKey("fk", categories);

            DatabaseSchemaFixer.UpdateReferences(schema);

            return schema;
        }
    }
}