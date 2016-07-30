using System.Data;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class EfCoreContextWriterTest
    {
        /// <summary>
        ///A test for Execute with CodeFirst
        ///</summary>
        [TestMethod]
        public void ExecuteTest()
        {
            //arrange
            var schema = PrepareModel();
            var settings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEfCore};
            var target = new CodeFirstContextWriter(settings);

            //act
            var result = target.Write(schema.Tables);

            //assert
            var hasDbSet = result.Contains("public DbSet<Product> ProductCollection");
            var hasOnModelCreating = result.Contains("protected override void OnModelCreating(ModelBuilder modelBuilder)");
            var hasMapping = result.Contains("modelBuilder.Entity<Product>(ProductMapping.Map);");
            Assert.IsTrue(hasDbSet);
            Assert.IsTrue(hasOnModelCreating);
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
