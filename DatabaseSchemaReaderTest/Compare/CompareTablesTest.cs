using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Compare
{
    [TestClass]
    public class CompareTablesTest
    {
        private static DatabaseTable CreateTable()
        {
            var table = new DatabaseTable();
            table.Name = "Test";
            table.AddColumn("A", DbType.Int32).AddPrimaryKey("PK_TEST")
                .AddColumn("B", DbType.Int32)
                .AddColumn("C", DbType.String).AddLength(10).AddNullable();

            return table;
        }

        [TestMethod]
        public void WhenTablesIdentical()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareTables(sb, writer);

            var baseTables = new List<DatabaseTable> { CreateTable() };
            var compareTables = new List<DatabaseTable> { CreateTable() };

            //act
            target.Execute(baseTables, compareTables);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(result));
        }

        /// <summary>
        /// When the table primary key changes columns
        /// </summary>
        [TestMethod]
        public void WhenTablePrimaryKeyChanged()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareTables(sb, writer);

            var baseTables = new List<DatabaseTable> { CreateTable() };
            var table = CreateTable().AddColumn("D", DbType.Int32).Table;
            table.PrimaryKey.Columns.Clear();
            table.PrimaryKey.Columns.Add("D");//the primary key is D, not A
            var compareTables = new List<DatabaseTable> { table };

            //act
            target.Execute(baseTables, compareTables);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("ALTER TABLE [Test] ADD [D] INT"), "add the new column");
            Assert.IsTrue(result.Contains("ALTER TABLE [Test] DROP CONSTRAINT [PK_TEST]"), "drop the old pk");
            Assert.IsTrue(result.Contains("ALTER TABLE [Test] ADD CONSTRAINT [PK_TEST] PRIMARY KEY ([D])"), "add the new pk");
        }


        [TestMethod]
        public void WhenTableUniqueConstraintChanged()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareTables(sb, writer);

            var baseTable = CreateTable();
            baseTable.FindColumn("B").AddUniqueKey("UK_TEST");
            var baseTables = new List<DatabaseTable> { baseTable };
            var compareTable = CreateTable().AddColumn("D", DbType.Int32).Table;
            compareTable.FindColumn("D").AddUniqueKey("UK_TEST");
            var compareTables = new List<DatabaseTable> { compareTable };

            //act
            target.Execute(baseTables, compareTables);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("ALTER TABLE [Test] ADD [D] INT"), "add the new column");
            Assert.IsTrue(result.Contains("ALTER TABLE [Test] DROP CONSTRAINT [UK_TEST]"), "drop the old unique key");
            Assert.IsTrue(result.Contains("ALTER TABLE [Test] ADD CONSTRAINT [UK_TEST] UNIQUE ([D])"), "add the new unique key");
        }

    }
}
