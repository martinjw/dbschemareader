using System.Text;
using System.Collections.Generic;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
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
    public class CompareProceduresTest
    {
        [TestMethod]
        public void WhenProceduresIdentical()
        {
            //arrange
            var sb = new StringBuilder();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareProcedures(sb, writer);

            var baseProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };
            var compareProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };

            //act
            target.Execute(baseProcedures, compareProcedures);
            var result = sb.ToString();

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void WhenViewDropped()
        {
            //arrange
            var sb = new StringBuilder();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareProcedures(sb, writer);

            var baseProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };
            var compareProcedures = new List<DatabaseStoredProcedure>();

            //act
            target.Execute(baseProcedures, compareProcedures);
            var result = sb.ToString();

            //assert
            Assert.IsTrue(result.Contains("DROP PROCEDURE"));
        }

        [TestMethod]
        public void WhenViewAdded()
        {
            //arrange
            var sb = new StringBuilder();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareProcedures(sb, writer);

            var baseProcedures = new List<DatabaseStoredProcedure>();
            var compareProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };

            //act
            target.Execute(baseProcedures, compareProcedures);
            var result = sb.ToString();

            //assert
            Assert.IsTrue(result.Contains("CREATE PROCEDURE"));
        }


        [TestMethod]
        public void WhenViewChanged()
        {
            //arrange
            var sb = new StringBuilder();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareProcedures(sb, writer);

            var baseProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };
            var view = CreateProcedure();
            view.Sql += " ORDER BY NAME";
            var compareProcedures = new List<DatabaseStoredProcedure> { view };

            //act
            target.Execute(baseProcedures, compareProcedures);
            var result = sb.ToString();

            //assert
            Assert.IsTrue(result.Contains("DROP PROCEDURE"));
            Assert.IsTrue(result.Contains("CREATE PROCEDURE"));
        }

        private static DatabaseStoredProcedure CreateProcedure()
        {
            return new DatabaseStoredProcedure { Name = "MyProcedure", Sql = "CREATE PROCEDURE MyProc AS SELECT * FROM TABLE" };
        }
    }
}
