using System.Linq;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Compare
{
    [TestClass]
    public class CompareSchemasIntegrationTest
    {

        [TestMethod]
        public void WhenFirstSchemaIsEmptyThenEverythingIsAdded()
        {
            //arrange
            var schema1 = new DatabaseSchema(null, null);
            var schema2 = TestHelper.GetNorthwindReader().ReadAll();

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.AreNotEqual(string.Empty, script);
            if (schema1.Tables.Any())
            {
                Assert.IsTrue(script.Contains("CREATE TABLE"));
            }
            if (schema1.Views.Any())
            {
                Assert.IsTrue(script.Contains("CREATE VIEW"));
            }
            if (schema1.StoredProcedures.Any())
            {
                Assert.IsTrue(script.Contains("CREATE PROCEDURE"));
            }
        }

        [TestMethod]
        public void WhenSecondSchemaIsEmptyThenEverythingIsDropped()
        {
            //arrange
            var schema1 = TestHelper.GetNorthwindReader().ReadAll();
            var schema2 = new DatabaseSchema(null, null);

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.AreNotEqual(string.Empty, script);
            if (schema1.Tables.Any())
            {
                Assert.IsTrue(script.Contains("DROP TABLE"));
            }
            if (schema1.Views.Any())
            {
                Assert.IsTrue(script.Contains("DROP VIEW"));
            }
            if (schema1.StoredProcedures.Any())
            {
                Assert.IsTrue(script.Contains("DROP PROCEDURE"));
            }
        }
        
    }
}
