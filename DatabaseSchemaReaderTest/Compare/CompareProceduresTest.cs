using System;
using System.Linq;
using System.Collections.Generic;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Compare
{
    [TestClass]
    public class CompareProceduresTest
    {
        [TestMethod]
        public void WhenProceduresIdentical()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareProcedures(sb, writer);

            var baseProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };
            var compareProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };

            //act
            target.Execute(baseProcedures, compareProcedures);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void WhenProcedureDropped()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareProcedures(sb, writer);

            var baseProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };
            var compareProcedures = new List<DatabaseStoredProcedure>();

            //act
            target.Execute(baseProcedures, compareProcedures);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("DROP PROCEDURE"));
        }

        [TestMethod]
        public void WhenProcedureAdded()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareProcedures(sb, writer);

            var baseProcedures = new List<DatabaseStoredProcedure>();
            var compareProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };

            //act
            target.Execute(baseProcedures, compareProcedures);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("CREATE PROCEDURE"));
        }


        [TestMethod]
        public void WheProcedureChanged()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareProcedures(sb, writer);

            var baseProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };
            var sproc = CreateProcedure();
            const string orderByName = " ORDER BY NAME";
            sproc.Sql += orderByName;
            var compareProcedures = new List<DatabaseStoredProcedure> { sproc };

            //act
            target.Execute(baseProcedures, compareProcedures);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("DROP PROCEDURE"));
            Assert.IsTrue(result.Contains("CREATE PROCEDURE"));
            Assert.IsTrue(result.Contains(orderByName));
        }


        [TestMethod]
        public void WheProcedureChangedInverse()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareProcedures(sb, writer);

            var sproc = CreateProcedure();
            const string orderByName = " ORDER BY NAME";
            sproc.Sql += orderByName;
            var baseProcedures = new List<DatabaseStoredProcedure> { sproc };
            var compareProcedures = new List<DatabaseStoredProcedure> { CreateProcedure() };

            //act
            target.Execute(baseProcedures, compareProcedures);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("DROP PROCEDURE"));
            Assert.IsTrue(result.Contains("CREATE PROCEDURE"));
            Assert.IsFalse(result.Contains(orderByName));
        }

        private static DatabaseStoredProcedure CreateProcedure()
        {
            return new DatabaseStoredProcedure { Name = "MyProcedure", Sql = "CREATE PROCEDURE MyProc AS SELECT * FROM TABLE" };
        }
    }
}
