using System.Reflection;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using DatabaseSchemaReader.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Utilities.EF
{
    [TestClass]
    public class EntityImporterTest
    {
        [TestMethod]
        public void TestImporterV3()
        {
            //arrange

            //we have a EF v5 edmx (v3) as an embedded resource (this project is only EF 4)
            const string streamPath = "DatabaseSchemaReaderTest.Utilities.EF.CatalogV3.edmxv3";

            var stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream(streamPath);
            if (stream == null) Assert.IsTrue(false, "Cannot access edmxv3 embedded resource");
            var doc = XDocument.Load(XmlReader.Create(stream));

            //load entity framework into DSR
            var importer = new EntityFrameworkImporter();
            var schema = importer.ReadEdmx(doc);
            var schema2 = importer.ReadEdmx(doc);


            //act
            //use our comparer to create migration script
            var comparer = new DatabaseSchemaReader.Compare.CompareSchemas(schema, schema2);
            var sql = comparer.Execute();


            //assert
            Assert.IsTrue(string.IsNullOrEmpty(sql), "Should have no differences");
        }

        [TestMethod]
        public void TestImporter()
        {
            const string streamPath = "Utilities.EF.Catalog.ssdl";

            var stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream(streamPath);
            if (stream == null) Assert.IsTrue(false, "Cannot access ssdl embedded resource");
            var doc = XDocument.Load(XmlReader.Create(stream));

            //load entity framework into DSR
            var importer = new EntityFrameworkImporter();
            var schema = importer.ReadSsdl(doc);

            //we changed the length of one column
            ChangeMaxLength(doc);

            //load the new schema into DSR
            var schema2 = importer.ReadSsdl(doc);

            //use our comparer to create migration script
            var comparer = new DatabaseSchemaReader.Compare.CompareSchemas(schema, schema2);
            var sql = comparer.Execute();

            //(we could compare the EF model to the database
            //var schema3 =
            //    new DatabaseReader(
            //        @"data source=.\SQLEXPRESS;initial catalog=ProductCatalog;integrated security=True;multipleactiveresultsets=True;App=Test",
            //        SqlType.SqlServer).ReadAll();
            //var comparer2 = new DatabaseSchemaReader.Compare.CompareSchemas(schema, schema3);
            //var sql2 = comparer2.Execute();
            //Console.WriteLine(sql2);


            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE [dbo].[Categories] ALTER COLUMN [CategoryName] NVARCHAR (100)"), "Should generate alter table DDL");

        }

        private static void ChangeMaxLength(XDocument doc)
        {
            XNamespace ssdlSchema = "http://schemas.microsoft.com/ado/2009/02/edm/ssdl";

            const string tableName = "Categories"; //it's pluralized here
            var entityType = doc.Root.Elements(ssdlSchema + "EntityType")
                .Where(et => (string) et.Attribute("Name") == tableName)
                .FirstOrDefault();
            if (entityType == null)
                Assert.Inconclusive("EDMX is changed");
            var prop = entityType.Elements(ssdlSchema + "Property")
                .Where(p => (string) p.Attribute("Name") == "CategoryName").First();
            prop.SetAttributeValue("MaxLength", 100);
        }
    }
}
