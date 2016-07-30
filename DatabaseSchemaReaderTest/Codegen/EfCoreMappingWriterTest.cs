using System;
using System.Data;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class EfCoreMappingWriterTest
    {
        /// <summary>
        ///A test for Execute with CodeFirst
        ///</summary>
        [TestMethod]
        public void ExecuteTest()
        {
            //arrange
            DatabaseSchema schema = PrepareModel();
            var products = schema.FindTableByName("Products");
            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEfCore };
            var target = new CodeFirstMappingWriter(products, codeWriterSettings, new MappingNamer());

            //act
            var result = target.Write();

            //assert
            var hasMappingClass = result.Contains("public static class ProductMapping");
            //we don't have the UseForeignKeyIdProperties=true, so map back to the instance property

            //EF v6 code
            //            HasRequired(x => x.SupplierKey).WithMany(c => c.ProductCollection).Map(m => m.MapKey("SupplierKey"));
            //            // Navigation properties
            //            // Many to many foreign key to Categories
            //            HasMany(x => x.CategoryCollection).WithMany(z => z.ProductCollection)
            //            .Map(map => 
            //            {
            //                map.ToTable("CategoryProducts");
            //                map.MapLeftKey("ProductId");
            //                map.MapRightKey("CategoryId");
            //            }


            var hasForeignKey =
                result.Contains("b.HasOne(x => x.SupplierKey).WithMany(c => c.ProductCollection);");
            //we have to create a many to many table for this
            //var hasManyToMany = result.Contains("HasMany(x => x.CategoryCollection).WithMany(z => z.ProductCollection)");

            Assert.IsTrue(hasMappingClass);
            Assert.IsTrue(hasForeignKey);
            //Assert.IsTrue(hasManyToMany);
        }



        private static DatabaseSchema PrepareModel()
        {
            var schema = new DatabaseSchema(null, null);

            schema.AddTable("Categories")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddColumn("CategoryName", DbType.String);

            schema.AddTable("Suppliers")
                .AddColumn("SupplierId", DbType.Int32).AddPrimaryKey()
                .AddColumn("SupplierName", DbType.String);

            schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey().AddIdentity()
                .AddColumn("ProductName", DbType.String)
                .AddColumn("SupplierKey", DbType.Int32).AddForeignKey("fk", "Suppliers");

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
