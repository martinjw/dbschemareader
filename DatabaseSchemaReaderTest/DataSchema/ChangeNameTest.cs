using System;
using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class ChangeNameTest
    {
        [TestMethod]
        public void ChangeTableNames()
        {
            var schema = BuildSchema();

            var cats = schema.FindTableByName("Categories");
            cats.ChangeName("DOGS");

            var prods = schema.FindTableByName("Products");
            prods.ChangeName("SPIDERS");

            var gen = new DdlGeneratorFactory(SqlType.Oracle);
            var tabgen = gen.AllTablesGenerator(schema);
            tabgen.IncludeSchema = false;
            var s = tabgen.Write();
            
            Assert.IsTrue(s.Contains("DOGS"));
            Assert.IsTrue(s.Contains("SPIDERS"));
            Assert.IsFalse(s.Contains("Categories"));
            Assert.IsFalse(s.Contains("Products"));
        }

        [TestMethod]
        public void ChangeColumnNames()
        {
            var schema = BuildSchema();

            var cats = schema.FindTableByName("Categories");
            cats.ChangeName("DOGS");
            cats.FindColumn("CategoryId").ChangeName("DogId");

            var prods = schema.FindTableByName("Products");
            prods.FindColumn("CategoryId").ChangeName("DogId");

            var gen = new DdlGeneratorFactory(SqlType.Oracle);
            var tabgen = gen.AllTablesGenerator(schema);
            tabgen.IncludeSchema = false;
            var s = tabgen.Write();

            Assert.IsTrue(s.Contains("DogId"));
            Assert.IsFalse(s.Contains("CategoryId"));
        }


        private static DatabaseSchema BuildSchema()
        {
            var schema = new DatabaseSchema(null, "Dummy");
            schema
                .AddTable("Categories")
                //chaining adding pk and identity
                .AddColumn("CategoryId", "INT").AddPrimaryKey().AddIdentity()
                //chaining from one column to next, with full access to the new column
                .AddColumn("CategoryName", "VARCHAR", c => c.Length = 30)
                .AddIndex("IndexByName")

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
                .AddColumn(new DatabaseColumn {Name = "Price", DbDataType = "DECIMAL", Nullable = true})
                //adding a fk
                .AddColumn("CategoryId", "INT")
                .AddForeignKey("FK_CATEGORY", tables => tables.Where(x => x.Name == "Categories").First());
            return schema;
        }
    }
}
