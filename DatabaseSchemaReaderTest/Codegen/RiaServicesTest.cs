using System;
using System.Data;
using System.IO;
using System.Linq;
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
    ///Test the data annotations
    ///</summary>
    [TestClass]
    public class RiaServicesTest
    {
        [TestMethod]
        public void TestRiaServices()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);

            schema.AddTable("Categories")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddColumn("CategoryName", DbType.String);

            schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey()
                .AddColumn("ProductName", DbType.String)
                .AddColumn("CategoryId", DbType.Int32).AddForeignKey("fk", "Categories");

            DatabaseSchemaFixer.UpdateReferences(schema);

            var directory = CreateDirectory("MyTest");
            const string @namespace = "MyTest";
            var settings = new CodeWriterSettings { Namespace = @namespace, CodeTarget = CodeTarget.PocoRiaServices };

            //act
            var target = new CodeWriter(schema, settings);
            target.Execute(directory);

            //assert
            var files = directory.GetFiles("*.cs");
            var products = files.FirstOrDefault(f => f.Name == "Product.cs");
            Assert.IsNotNull(products, "Should have written Product class to represent [Products] table");

            var category = files.FirstOrDefault(f => f.Name == "Category.cs");
            Assert.IsNotNull(category, "Should have written Category class to represent [Categories] table");

            var cs = File.ReadAllText(category.FullName);

            var ok = cs.Contains("[MetadataType(typeof(Category.CategoryMetadata))]");
            Assert.IsTrue(ok, "Should contain nested metadata class");
        }

        private static DirectoryInfo CreateDirectory(string folder)
        {
            var directory = new DirectoryInfo(Environment.CurrentDirectory);
            if (directory.GetDirectories(folder).Any())
            {
                //if it's already there, clear it out
                var sub = directory.GetDirectories(folder).First();
                sub.Delete(true);
            }
            return directory.CreateSubdirectory(folder);
        }
    }
}
