using System.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReaderTest.IntegrationTests;
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

namespace DatabaseSchemaReaderTest.Compare
{
    [TestClass]
    public class CompareSchemasIntegrationTest
    {
        private static DatabaseReader GetNortwindReader()
        {
            const string providername = "System.Data.SqlClient";
            const string connectionString = ConnectionStrings.Northwind;
            ProviderChecker.Check(providername, connectionString);

            return new DatabaseReader(connectionString, providername);
        }

        [TestMethod]
        public void WhenFirstSchemaIsEmptyThenEverythingIsAdded()
        {
            //arrange
            var schema1 = new DatabaseSchema(null, null);
            var schema2 = GetNortwindReader().ReadAll();

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
            var schema1 = GetNortwindReader().ReadAll();
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
