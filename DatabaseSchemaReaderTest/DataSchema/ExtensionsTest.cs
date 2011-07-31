using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class ExtensionsTest
    {
        [TestMethod]
        public void TestExtensions()
        {
            //a simple fluent interface for creating the schema in memory
            var schema = new DatabaseSchema(null, null);
            schema
                .AddTable("Categories")
                //chaining adding pk and identity
                .AddColumn("CategoryId", "INT").AddPrimaryKey().AddIdentity()
                //chaining from one column to next, with full access to the new column
                .AddColumn("CategoryName", "VARCHAR", c => c.Length = 30)

                //chaining from a column to the next table
                .AddTable("Products")
                .AddColumn("ProductId", "INT").AddIdentity().AddPrimaryKey("PK_PRODUCTS")
                //add additional properties to column
                .AddColumn("ProductName", "VARCHAR", c =>
                                                         {
                                                             c.Length = 30;
                                                             c.Nullable = true;
                                                         })
                //adding a column directly
                .AddColumn(new DatabaseColumn { Name = "Price", DbDataType = "DECIMAL", Nullable = true })
                //adding a fk
                .AddColumn("CategoryId", "INT")
                    .AddForeignKey("FK_CATEGORY", tables => tables.Where(x => x.Name == "Categories").First());

            //assert
            Assert.AreEqual(2, schema.Tables.Count);

            var cats = schema.FindTableByName("Categories");
            Assert.AreEqual(2, cats.Columns.Count);
            Assert.IsNotNull(cats.PrimaryKey);
            Assert.IsNotNull(cats.PrimaryKeyColumn);
            var pk = cats.PrimaryKeyColumn;
            Assert.IsTrue(pk.IsIdentity);
            Assert.AreEqual(1, cats.ForeignKeyChildren.Count);


            var prods = schema.FindTableByName("Products");
            Assert.AreEqual(4, prods.Columns.Count);
            Assert.AreEqual(1, prods.ForeignKeys.Count);
            var fk = prods.ForeignKeys[0];
            Assert.AreEqual(cats, fk.ReferencedTable(schema));
        }

        [TestMethod]
        public void UniqueKeysTest()
        {
            var schema = new DatabaseSchema(null, null);
            schema
                .AddTable("Categories")
                .AddColumn("CategoryId", "INT").AddPrimaryKey().AddIdentity()
                .AddColumn("CategoryName", "VARCHAR", c => c.Length = 30).AddUniqueKey("UK_NAME");

            //assert
            var cats = schema.FindTableByName("Categories");

            Assert.AreEqual(1, cats.UniqueKeys.Count);
            var uk = cats.UniqueKeys[0];
            Assert.AreEqual("UK_NAME", uk.Name);
            Assert.AreEqual("CategoryName", uk.Columns.Single());

            var catName = cats.Columns.Find(c => c.Name == "CategoryName");
            Assert.IsTrue(catName.IsUniqueKey);

        }
    }
}
