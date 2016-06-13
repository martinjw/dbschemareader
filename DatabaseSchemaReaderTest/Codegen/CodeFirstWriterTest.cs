using System;
using System.Data;
using System.IO;
using System.Linq;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{


    /// <summary>
    /// Test code first generation
    ///</summary>
    [TestClass]
    public class CodeFirstWriterTest
    {

        /// <summary>
        ///A test for Execute with CodeFirst
        ///</summary>
        [TestMethod]
        public void ExecuteTest()
        {
            DatabaseSchema schema = PrepareModel();
            const string @namespace = "MyTest";
            var settings = new CodeWriterSettings { Namespace = @namespace, CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var target = new CodeWriter(schema, settings);

            var directory = TestHelper.CreateDirectory("MyTest");

            target.Execute(directory);

            //assert
            var mapping = directory.GetDirectories("mapping").FirstOrDefault();
            Assert.IsNotNull(mapping);

            var files = mapping.GetFiles("*.cs");
            var products = files.FirstOrDefault(f => f.Name == "ProductMapping.cs");
            Assert.IsNotNull(products, "Should have written Product Mapping class");

            var cs = File.ReadAllText(products.FullName);

            var ok = cs.Contains("EntityTypeConfiguration<Product>");
            Assert.IsTrue(ok, "Should contain an EntityTypeConfiguration class mapper");
        }

        private static DatabaseSchema PrepareModel()
        {
            var schema = new DatabaseSchema(null, null);

            schema.AddTable("Categories")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddColumn("CategoryName", DbType.String);

            schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey()
                .AddColumn("ProductName", DbType.String)
                .AddColumn("CategoryId", DbType.Int32).AddForeignKey("fk", "Categories");

            DatabaseSchemaFixer.UpdateReferences(schema);

            return schema;
        }
    }
}
