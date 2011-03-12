using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReader.CodeGen;
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

namespace DatabaseSchemaReaderTest.Codegen
{


    /// <summary>
    ///Create a simple model and write it to filesystem
    ///</summary>
    [TestClass]
    public class CodeWriterTest
    {
        private static DatabaseReader GetNortwindReader()
        {
            const string providername = "System.Data.SqlClient";
            const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
            ProviderChecker.Check(providername, connectionString);

            return new DatabaseReader(connectionString, providername);
        }

        private static DirectoryInfo CreateDirectory(string folder)
        {
            var directory = new DirectoryInfo(Environment.CurrentDirectory);
            if (directory.GetDirectories(folder).Any())
            {
                //if it's already there, clear it out
                var sub = directory.GetDirectories(folder).First();
                sub.Delete(true);
            }
            return directory.CreateSubdirectory(folder);
        }

        [TestMethod]
        public void NorthwindTest()
        {
            var dbReader = GetNortwindReader();
            var schema = dbReader.ReadAll();

            var directory = CreateDirectory("Northwind");
            const string @namespace = "Northwind.Domain";

            var codeWriter = new CodeWriter(schema);
            codeWriter.Execute(directory, @namespace);

            var files = directory.GetFiles("*.cs");

            var category = files.First(f => f.Name == "Category.cs");
            var cs = File.ReadAllText(category.FullName);

            var ok = cs.Contains("public virtual IList<Product> ProductCollection { get; private set; }");
            Assert.IsTrue(ok, "Should contain the collection of products");

            /*
             * When generated, create a startup project-
             *  Reference NHibernate and Castle
             *  Add App.Config with NHibernate configuration
             *  Run the NH config in app startup - for test projects use something like this...
        private static ISession Initialize()
        {
            var configuration = new Configuration();
            configuration.Configure(); //configure from the app.config
            //reference one of your domain classes here
            configuration.AddAssembly(typeof(Category).Assembly);
            var sessionFactory = configuration.BuildSessionFactory();

            return sessionFactory.OpenSession();
        }
             * 
             */
        }

        [TestMethod]
        public void AdventureWorksTest()
        {
            const string providername = "System.Data.SqlClient";
            const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=AdventureWorks";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            DatabaseSchema schema = null;
            try
            {
                schema = dbReader.ReadAll();
            }
            catch (SqlException exception)
            {
                Assert.Inconclusive("Cannot access database " + exception.Message);
            }
            var directory = CreateDirectory("AdventureWorks");
            const string @namespace = "AdventureWorks.Domain";

            var codeWriter = new CodeWriter(schema);
            codeWriter.Execute(directory, @namespace);

            var procedures = directory.GetDirectories("Procedures").FirstOrDefault();
            if (procedures == null)
                Assert.Fail("Could not find Procedures subdirectory for stored procedures");
            var files = procedures.GetFiles("*.cs");

            var category = files.First(f => f.Name == "uspLogError.cs");
            var cs = File.ReadAllText(category.FullName);


            var ok = cs.Contains("public virtual DbCommand CreateCommand(int? errorLogId)");
            Assert.IsTrue(ok, "Should contain the uspLogError stored procedure (in standard AdventureWorks db)");
        }

        [TestMethod]
        public void OracleHrTest()
        {
            const string providername = "System.Data.OracleClient";
            const string connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=XE)));User Id=HR;Password=HR;";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            dbReader.Owner = "HR";
            DatabaseSchema schema = null;
            try
            {
                schema = dbReader.ReadAll();
            }
            catch (DbException exception)
            {
                Assert.Inconclusive("Cannot access database " + exception.Message);
            }
            var directory = CreateDirectory("Hr");
            const string @namespace = "Hr.Domain";

            var codeWriter = new CodeWriter(schema);
            codeWriter.Execute(directory, @namespace);

            var mapping = directory.GetDirectories("mapping").FirstOrDefault();
            if (mapping == null)
                Assert.Fail("Could not find Mapping subdirectory");
            var files = mapping.GetFiles("*.xml");

            var employeeMap = files.First(f => f.Name == "Employee.hbm.xml");
            var doc = XDocument.Load(employeeMap.FullName);

            var classElement = doc.Descendants("{urn:nhibernate-mapping-2.2}class").FirstOrDefault();
            Assert.AreEqual("Employee", (string)classElement.Attribute("name"));
            Assert.AreEqual("`EMPLOYEES`", (string)classElement.Attribute("table"));
        }


        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod]
        public void ExecuteTest()
        {
            DatabaseSchema schema = PrepareModel();
            var target = new CodeWriter(schema);

            var directory = CreateDirectory("MyTest");
            const string @namespace = "MyTest";

            target.Execute(directory, @namespace);

            var files = directory.GetFiles("*.cs");
            var products = files.FirstOrDefault(f => f.Name == "Product.cs");
            Assert.IsNotNull(products, "Should have written Product class to represent [Products] table");

            var category = files.FirstOrDefault(f => f.Name == "Category.cs");
            Assert.IsNotNull(products, "Should have written Category class to represent [Categories] table");

            var cs = File.ReadAllText(category.FullName);

            var ok = cs.Contains("public virtual IList<Product> ProductCollection { get; private set; }");
            Assert.IsTrue(ok, "Should contain the collection of products");
        }

        private static DatabaseSchema PrepareModel()
        {
            var schema = new DatabaseSchema(null, null);
            var integer = new DataType("INT", typeof(int).FullName);
            var @string = new DataType("VARCHAR", typeof(string).FullName);

            var categories = new DatabaseTable { Name = "Categories" };
            var categoryId = new DatabaseColumn { Name = "CategoryId", DataType = integer };
            var name = new DatabaseColumn { Name = "CategoryName", DataType = @string };
            categories.Columns.Add(categoryId);
            categories.Columns.Add(name);
            schema.Tables.Add(categories);

            var products = new DatabaseTable { Name = "Products" };
            var productId = new DatabaseColumn { Name = "ProductId", DataType = integer };
            var productName = new DatabaseColumn { Name = "ProductName", DataType = @string };
            var productCategory = new DatabaseColumn { Name = "CategoryId", DataType = integer, ForeignKeyTableName = "Categories", IsForeignKey = true };
            products.Columns.Add(productId);
            products.Columns.Add(productName);
            products.Columns.Add(productCategory);
            schema.Tables.Add(products);

            DatabaseSchemaFixer.UpdateReferences(schema);

            return schema;
        }
    }
}
