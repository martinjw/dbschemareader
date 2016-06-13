using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Utilities
{
    [TestClass]
    public class SchemaTablesSorterTest
    {

        [TestMethod]
        public void TestTopologicalSort()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);

            var orders = new DatabaseTable();
            orders.Name = "orders";
            var productsFk = new DatabaseConstraint
                                 {
                                     ConstraintType = ConstraintType.ForeignKey,
                                     RefersToTable = "products"
                                 };
            orders.AddConstraint(productsFk);
            schema.Tables.Add(orders);

            var categories = new DatabaseTable();
            categories.Name = "categories";
            schema.Tables.Add(categories);

            var products = new DatabaseTable();
            products.Name = "products";
            var categoriesFk = new DatabaseConstraint();
            categoriesFk.ConstraintType = ConstraintType.ForeignKey;
            categoriesFk.RefersToTable = "categories";
            products.AddConstraint(categoriesFk);
            schema.Tables.Add(products);

            //act
            var sortedTables = SchemaTablesSorter.TopologicalSort(schema);

            //assert
            var first = sortedTables.First();
            var last = sortedTables.Last();
            Assert.AreEqual(categories, first);
            Assert.AreEqual(orders, last);
        }

        [TestMethod]
        public void WithBidirectionalDepndencyTopologicalSort()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);

            var orders = new DatabaseTable();
            orders.Name = "countries";
            var productsFk = new DatabaseConstraint();
            productsFk.ConstraintType = ConstraintType.ForeignKey;
            productsFk.RefersToTable = "capitalcities";
            orders.AddConstraint(productsFk);
            schema.Tables.Add(orders);

            var products = new DatabaseTable();
            products.Name = "capitalcities";
            var categoriesFk = new DatabaseConstraint();
            categoriesFk.ConstraintType = ConstraintType.ForeignKey;
            categoriesFk.RefersToTable = "countries";
            products.AddConstraint(categoriesFk);
            schema.Tables.Add(products);

            //a country has one capital city
            //a capital city is in one country
            //But bidirectional foreign keys is terrible database design - you really only need one direction.
            //(you have to save the country with a null capital, then the capital, then update the country again).
            //Topological sorts don't support cycles, so we should just get back the original list

            //act
            var sortedTables = SchemaTablesSorter.TopologicalSort(schema);

            //assert
            Assert.AreEqual(2, sortedTables.Count());
            //non-deterministic order
        }
    }
}
