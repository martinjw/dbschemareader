using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Filters;
using DatabaseSchemaReader.Procedures;
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
using TestCategory = NUnit.Framework.CategoryAttribute;
#endif

namespace DatabaseSchemaReaderTest.Codegen
{
    /// <summary>
    ///Create a simple model and write it to filesystem
    ///</summary>
    [TestClass]
    public class CodeWriterIntegrationTest
    {
        [TestMethod, TestCategory("SqlServer")]
        public void NorthwindTest()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();

            var directory = TestHelper.CreateDirectory("Northwind");
            const string @namespace = "Northwind.Domain";
            var settings = new CodeWriterSettings { Namespace = @namespace, CodeTarget = CodeTarget.Poco };

            var codeWriter = new CodeWriter(schema, settings);
            codeWriter.Execute(directory);

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

        [TestMethod, TestCategory("SqlServer")]
        public void NorthwindViewTest()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();

            var directory = TestHelper.CreateDirectory("NorthwindView");
            const string @namespace = "Northwind.Domain";
            var settings = new CodeWriterSettings { Namespace = @namespace, CodeTarget = CodeTarget.Poco, IncludeViews = true };

            var codeWriter = new CodeWriter(schema, settings);
            codeWriter.Execute(directory);

            var files = directory.GetFiles("*.cs");

            var categorySalesView = files.FirstOrDefault(f => f.Name == "CategorySalesFor1997.cs");
            Assert.IsNotNull(categorySalesView);
        }

        [TestMethod, TestCategory("SqlServer")]
        public void NorthwindProcedureTest()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            //remove the spCreateDiagram system sprocs
            dbReader.Exclusions.StoredProcedureFilter = new PrefixFilter("sp");
            var schema = dbReader.ReadAll();

            var procedure = schema.StoredProcedures
                .Find(x => string.Equals(x.Name, "Employee Sales by Country", StringComparison.OrdinalIgnoreCase));
            if (procedure == null)
            {
                Assert.Inconclusive("No Employee Sales By Country found in Northwind");
            }

            //getting the procedure resultsets is a special call
            var runner = new ResultSetReader(schema);
            runner.Execute();

            var directory = TestHelper.CreateDirectory("NorthwindSproc");
            const string @namespace = "Northwind.Domain";
            var settings = new CodeWriterSettings
            {
                Namespace = @namespace,
                CodeTarget = CodeTarget.Poco,
                WriteStoredProcedures = true
            };

            var codeWriter = new CodeWriter(schema, settings);
            codeWriter.Execute(directory);

            var procedures = directory.GetDirectories("Procedures").FirstOrDefault();
            if (procedures == null)
                Assert.Fail("Could not find Procedures subdirectory");
            var files = procedures.GetFiles("*.cs");
            var employeeSales = files.FirstOrDefault(f => f.Name == "EmployeeSalesByCountry.cs");
            Assert.IsNotNull(employeeSales, "Should have written EmployeeSalesByCountry class for Employee Sales By Country procedure");

            var cs = File.ReadAllText(employeeSales.FullName);
            Assert.IsTrue(cs.Contains("public virtual IEnumerable<EmployeeSalesByCountryResult> Execute(DateTime? beginningDate, DateTime? endingDate)"), "Generated input signature");

            var employeeSalesResult = files.FirstOrDefault(f => f.Name == "EmployeeSalesByCountryResult.cs");
            Assert.IsNotNull(employeeSalesResult, "Should have written EmployeeSalesByCountryResult class for Employee Sales By Country procedure");

            cs = File.ReadAllText(employeeSalesResult.FullName);
            Assert.IsTrue(cs.Contains("public virtual string Country { get; set; }"), "Generated property for resultSet column");
        }

        [TestMethod, TestCategory("SqlServer.AdventureWorks")]
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
            var directory = TestHelper.CreateDirectory("AdventureWorks");
            const string @namespace = "AdventureWorks.Domain";
            var settings = new CodeWriterSettings { Namespace = @namespace, CodeTarget = CodeTarget.Poco, WriteStoredProcedures = true };

            var codeWriter = new CodeWriter(schema, settings);
            codeWriter.Execute(directory);

            var procedures = directory.GetDirectories("Procedures").FirstOrDefault();
            if (procedures == null)
                Assert.Fail("Could not find Procedures subdirectory for stored procedures");
            var files = procedures.GetFiles("*.cs");

            var category = files.First(f => f.Name == "uspLogError.cs");
            var cs = File.ReadAllText(category.FullName);

            var ok = cs.Contains("public virtual DbCommand CreateCommand(int? errorLogId)");
            Assert.IsTrue(ok, "Should contain the uspLogError stored procedure (in standard AdventureWorks db)");
        }

        [TestMethod, TestCategory("Oracle")]
        public void OracleHrTest()
        {
            const string providername = "System.Data.OracleClient";
            const string connectionString = ConnectionStrings.OracleHr;
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
            var directory = TestHelper.CreateDirectory("Hr");
            const string @namespace = "Hr.Domain";
            var settings = new CodeWriterSettings { Namespace = @namespace, CodeTarget = CodeTarget.PocoNHibernateHbm };

            var codeWriter = new CodeWriter(schema, settings);
            codeWriter.Execute(directory);

            var mapping = directory.GetDirectories("mapping").FirstOrDefault();
            if (mapping == null)
                Assert.Fail("Could not find Mapping subdirectory");
            var files = mapping.GetFiles("*.xml");

            var employeeMap = files.First(f => f.Name == "Employee.hbm.xml");
            var doc = XDocument.Load(employeeMap.FullName);

            var classElement = doc.Descendants("{urn:nhibernate-mapping-2.2}class").First();
            Assert.AreEqual("Employee", (string)classElement.Attribute("name"));
            Assert.AreEqual("`EMPLOYEES`", (string)classElement.Attribute("table"));
        }
    }
}