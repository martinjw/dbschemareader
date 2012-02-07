using System.Data;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
#endif

namespace DatabaseSchemaReaderTest.Codegen
{


    /// <summary>
    ///Test code first mapping
    ///</summary>
    [TestClass]
    public class CodeFirstMappingWriterTest
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
            var target = new CodeFirstMappingWriter(products, "Domain");

            //act
            var result = target.Write();

            //assert
            var hasMappingClass = result.Contains("public class ProductMapping : EntityTypeConfiguration<Product>");
            var hasForeignKey = result.Contains("HasRequired(x => x.Supplier).WithMany(c => c.ProductCollection)");
            var hasManyToMany = result.Contains("HasMany(x => x.CategoryCollection).WithMany(z => z.ProductCollection)");

            Assert.IsTrue(hasMappingClass);
            Assert.IsTrue(hasForeignKey);
            Assert.IsTrue(hasManyToMany);
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
                .AddColumn("SupplierId", DbType.Int32).AddForeignKey("fk", "Suppliers");

            schema.AddTable("CategoryProducts")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddForeignKey("fk", "Categories")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey()
                .AddForeignKey("fk", "Products");

            DatabaseSchemaFixer.UpdateReferences(schema);
            PrepareSchemaNames.Prepare(schema);

            return schema;
        }
    }
}
