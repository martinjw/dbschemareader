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
    /// Test class writer
    ///</summary>
    [TestClass]
    public class ClassWriterTest
    {


        /// <summary>
        ///A test for write
        ///</summary>
        [TestMethod]
        public void WriteTest()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            var table = schema.AddTable("Categories")
                .AddColumn("CategoryId", "INT").AddPrimaryKey().AddIdentity()
                .AddColumn("CategoryName", "NVARCHAR").Table;
            //we need datatypes
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            schema.DataTypes.Add(new DataType("NVARCHAR", "System.String"));
            DatabaseSchemaFixer.UpdateDataTypes(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema);

            var cw = new ClassWriter(table, "Domain");

            //act
            var txt = cw.Write();

            //assert
            var hasName = txt.Contains("public class Category");
            var hasCategoryId = txt.Contains("public virtual int CategoryId { get; set; }");
            var hasCategoryName = txt.Contains("public virtual string CategoryName { get; set; }");

            Assert.IsTrue(hasName);
            Assert.IsTrue(hasCategoryId);
            Assert.IsTrue(hasCategoryName);
        }

        private static DatabaseSchema ArrangeSchema()
        {
            var schema = new DatabaseSchema(null, null);
            schema
                .AddTable("Categories")
                .AddColumn("CategoryId", "INT").AddPrimaryKey().AddIdentity()
                .AddColumn("CategoryName", "NVARCHAR")
                .AddTable("Products")
                .AddColumn("ProductId", "INT").AddPrimaryKey()
                .AddColumn("ProductName", "NVARCHAR")
                .AddColumn("CategoryId", "INT").AddForeignKey("fk", "Categories");
            //we need datatypes
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            schema.DataTypes.Add(new DataType("NVARCHAR", "System.String"));
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema);
            return schema;
        }

        [TestMethod]
        public void WriteForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var productsTable = schema.FindTableByName("Products");

            var cw = new ClassWriter(productsTable, "Domain");

            //act
            var txt = cw.Write();

            //assert
            var hasCategory = txt.Contains("public virtual Category Category { get; set; }");

            Assert.IsTrue(hasCategory);
        }

        [TestMethod]
        public void WriteForeignKeyInverseTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var categoryTable = schema.FindTableByName("Categories");
            
            var cw = new ClassWriter(categoryTable, "Domain");

            //act
            var txt = cw.Write();

            //assert
            var hasProducts = txt.Contains("public virtual IList<Product> ProductCollection { get; private set; }");

            Assert.IsTrue(hasProducts);
        }

        [TestMethod]
        public void WriteForeignKeyInverseWithCollectionNamerTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var categoryTable = schema.FindTableByName("Categories");

            var cw = new ClassWriter(categoryTable, "Domain");
            cw.CollectionNamer = new PluralizingNamer();

            //act
            var txt = cw.Write();

            //assert
            var hasProducts = txt.Contains("public virtual IList<Product> Products { get; private set; }");

            Assert.IsTrue(hasProducts);
        }


        [TestMethod]
        public void WriteCodeFirstTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var categoryTable = schema.FindTableByName("Categories");

            var cw = new ClassWriter(categoryTable, "Domain");
            cw.CodeTarget = CodeTarget.PocoEntityCodeFirst;

            //act
            var txt = cw.Write();

            //assert
            var hasCategoryId = txt.Contains("public int CategoryId { get; set; }");
            var hasProducts = txt.Contains("public virtual ICollection<Product> ProductCollection { get; private set; }");

            Assert.IsTrue(hasCategoryId, "Ordinary scalar properties don't need to be virtual");
            Assert.IsTrue(hasProducts);
        }

    }
}
