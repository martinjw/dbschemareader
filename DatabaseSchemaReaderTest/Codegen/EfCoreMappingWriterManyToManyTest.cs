using System;
using System.Data;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class EfCoreMappingWriterManyToManyTest
    {
        private readonly DatabaseSchema _schema;
        public EfCoreMappingWriterManyToManyTest()
        {
            _schema = PrepareModel();
        }

        /// <summary>
        ///A test for Execute with CodeFirst
        ///</summary>
        [TestMethod]
        public void JoinTableCreated()
        {
            //arrange
            var products = _schema.FindTableByName("CategoryProducts");
            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEfCore };
            var target = new CodeFirstMappingWriter(products, codeWriterSettings, new MappingNamer());

            //act
            var result = target.Write();

            //assert
            //        public static void Map(EntityTypeBuilder<CategoryProduct> b)
            //        {
            //            //table
            //            b.ToTable("CategoryProducts");
            //            // Primary key (composite)
            //            b.HasKey(x => new { x.CategoryId, x.ProductId });
            //            // Properties
            //            b.HasOne(x => x.Category).WithOne();
            //            b.HasOne(x => x.Product).WithOne();


            var hasKey =
                result.Contains("b.HasKey(x => new { x.CategoryId, x.ProductId });");
            //we have to create a many to many table for this
            var hasCategory = result.Contains("b.HasOne(x => x.Category).WithOne(");
            var hasProduct = result.Contains("b.HasOne(x => x.Product).WithOne(");

            Assert.IsTrue(hasKey);
            Assert.IsTrue(hasCategory);
            Assert.IsTrue(hasProduct);
        }

        /// <summary>
        ///A test for Execute with CodeFirst
        ///</summary>
        [TestMethod]
        public void TableLinksToJoinTable()
        {
            //arrange
            var products = _schema.FindTableByName("Categories");
            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEfCore };
            var target = new CodeFirstMappingWriter(products, codeWriterSettings, new MappingNamer());

            //act
            var result = target.Write();

            //assert
            var hasCategory = result.Contains("b.HasMany(x => x.CategoryProductCollection).WithOne(");

            Assert.IsTrue(hasCategory);
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
