using System.Linq;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class DatabaseConstraintTest
    {
        [TestMethod]
        public void TestReferencedTableViaRefersToTable()
        {
            //create a schema
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            schema.AddTable("Categories")
                .AddColumn("CategoryId").AddPrimaryKey("CategoryPK")
                .AddColumn("CategoryName")

                .AddTable("Products")
                .AddColumn("ProductId").AddPrimaryKey()
                .AddColumn("ProductName")
                .AddColumn("CategoryId").AddForeignKey("Categories");

            //look at the schema
            var categories = schema.FindTableByName("Categories");
            var products = schema.FindTableByName("Products");
            var fk = products.ForeignKeys.First();

            //act
            var referencedTable = fk.ReferencedTable(schema);

            //assert
            Assert.AreEqual(categories, referencedTable);
        }

        [TestMethod]
        public void TestReferencedTableViaConstraintName()
        {
            //create a schema
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            schema.AddTable("Products")
                .AddColumn("ProductId").AddPrimaryKey()
                .AddColumn("ProductName")
                .AddColumn("CategoryId")
                
                .AddTable("Categories")
                .AddColumn("CategoryId").AddPrimaryKey("CategoryPK")
                .AddColumn("CategoryName")
                ;

            //look at the schema
            var categories = schema.FindTableByName("Categories");
            var products = schema.FindTableByName("Products");
            //from the database we normally get a RefersToTable defined.
            //sometimes we don't- we just get the name of the pk constraint
            //so here we simulate that
            var fk = new DatabaseConstraint
                         {
                             ConstraintType = ConstraintType.ForeignKey,
                             TableName = "Categories",
                             RefersToConstraint = "CategoryPK"
                         };
            fk.Columns.Add("CategoryId");
            products.AddConstraint(fk);

            //act
            var referencedTable = fk.ReferencedTable(schema);

            //assert
            Assert.AreEqual(categories, referencedTable);
        }
    }
}
