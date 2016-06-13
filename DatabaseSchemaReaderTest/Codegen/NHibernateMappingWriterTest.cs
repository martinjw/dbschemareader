using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.NHibernate;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{

    /// <summary>
    ///Test the NHibernate mapping (against the official xsd)
    ///</summary>
    [TestClass]
    public class NHibernateMappingWriterTest
    {
        [TestMethod]
        public void TestSimpleMapping()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);

            schema.AddTable("Categories")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddColumn("CategoryName", DbType.String);

            var products = schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey()
                .AddColumn("ProductName", DbType.String)
                .AddColumn("CategoryId", DbType.Int32).AddForeignKey("fk", "Categories")
                .Table;

            DatabaseSchemaFixer.UpdateReferences(schema);

            var settings = new CodeWriterSettings { Namespace = "MyTest", CodeTarget = CodeTarget.PocoNHibernateHbm };
            PrepareSchemaNames.Prepare(schema, settings.Namer);

            //act
            var target = new MappingWriter(products, settings);
            var txt = target.Write();

            //assert
            var errors = Validate(txt);
            Assert.IsFalse(errors);
        }

        [TestMethod]
        public void TestCompositeKey()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);

            var orderDetail = schema.AddTable("OrderDetails")
                .AddColumn("OrderID", DbType.Int32).AddPrimaryKey()
                .AddColumn("ProductID", DbType.Int32)
                .AddColumn<int>("UnitPrice")
                .Table;
            orderDetail.PrimaryKey.AddColumn(orderDetail.FindColumn("ProductID"));

            DatabaseSchemaFixer.UpdateReferences(schema);

            var settings = new CodeWriterSettings { Namespace = "MyTest", CodeTarget = CodeTarget.PocoNHibernateHbm };
            PrepareSchemaNames.Prepare(schema, settings.Namer);

            //act
            var target = new MappingWriter(orderDetail, settings);
            var txt = target.Write();

            //assert
            var errors = Validate(txt);
            Assert.IsFalse(errors);
        }

        [TestMethod]
        public void TestNaturalKey()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);

            var diagrams = schema.AddTable("Diagrams")
                .AddColumn("diagram_id", DbType.Int32).AddPrimaryKey()
                .AddColumn<string>("name").AddLength(10).AddUniqueKey()
                .AddColumn<int>("principal_id")
                .Table;
            diagrams.UniqueKeys.Single().AddColumn(diagrams.FindColumn("principal_id"));

            DatabaseSchemaFixer.UpdateReferences(schema);

            var settings = new CodeWriterSettings { Namespace = "MyTest", CodeTarget = CodeTarget.PocoNHibernateHbm };
            PrepareSchemaNames.Prepare(schema, settings.Namer);

            //act
            var target = new MappingWriter(diagrams, settings);
            var txt = target.Write();

            //assert
            var errors = Validate(txt);
            Assert.IsFalse(errors);
        }

        private static XmlSchemaSet CreateSchemas()
        {
            var stream =
                Assembly.GetExecutingAssembly()
                        .GetManifestResourceStream("DatabaseSchemaReaderTest.Codegen.nhibernate-mapping.xsd");
            var schemas = new XmlSchemaSet();
            schemas.Add("urn:nhibernate-mapping-2.2", XmlReader.Create(stream));
            return schemas;
        }


        private static bool Validate(string txt)
        {
            var doc = XDocument.Parse(txt);
            var schemas = CreateSchemas();

            bool errors = false;
            doc.Validate(schemas, (o, e) =>
            {
                //validation event handler
                Console.WriteLine("{0}", e.Message);
                errors = true;
            }, true);
            return errors;
        }
    }
}
