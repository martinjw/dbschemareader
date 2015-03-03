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

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class RemoveTableTest
    {
        [TestMethod]
        public void RemoveTable()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            schema
                .AddTable("Category")
                .AddColumn<int>("CategoryId").AddPrimaryKey()
                .AddColumn<string>("CategoryName").AddLength(50).AddNullable()

                .AddTable("Product")
                .AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<int>("CategoryId").AddForeignKey("Category")

                .AddTable("Vendor")
                .AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<int>("CategoryId").AddForeignKey("Category")
               ;

            //act
            schema.RemoveTable("Category");

            //assert
            Assert.AreEqual(2, schema.Tables.Count);
            var prod = schema.FindTableByName("Product");
            Assert.AreEqual(0, prod.ForeignKeys.Count);
            var categoryId = prod.FindColumn("CategoryId");
            Assert.IsFalse(categoryId.IsForeignKey);
        }
    }
}
