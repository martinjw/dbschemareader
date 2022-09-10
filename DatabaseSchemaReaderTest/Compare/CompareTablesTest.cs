using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
            Assert.IsTrue(result.Contains("ALTER TABLE [Test] DROP CONSTRAINT IF EXISTS [PK_TEST]"), "drop the old pk");
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
            Assert.IsTrue(result.Contains("ALTER TABLE [Test] DROP CONSTRAINT IF EXISTS [UK_TEST]"), "drop the old unique key");
            Assert.IsTrue(result.Contains("ALTER TABLE [Test] ADD CONSTRAINT [UK_TEST] UNIQUE ([D])"), "add the new unique key");
        }

        [TestMethod]
        public void WhenDroppedColumn()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareTables(sb, writer);
            var baseTable = CreateTable().AddColumn("D", DbType.Int32).AddIndex("idx_d").Table;
            var compareTable = CreateTable();
            var baseTables = new List<DatabaseTable> { baseTable };
            var compareTables = new List<DatabaseTable> { compareTable };

            //act
            target.Execute(baseTables, compareTables);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(result.Contains("DROP INDEX IF EXISTS [idx_d] ON [Test];"), "The index is dropped as well as the column");
            //normally we add a column, then add indexes/constraints
            //but when we drop, the order is reversed. we have to drop the indexes/constraints first (unless the db supports DROP CASCADE)
            //for SqlServer we can use IF EXISTS and repeat the drop index
            //for Oracle or MySql, there is no IF EXISTS, so the 2nd drop index will fail. You must wrap it with exception trapping.
            //Dropping columns is rare so this may not be a common issue in practice
        }

        [TestMethod]
        public void WhenTableIndexChanged()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareTables(sb, writer);

            var baseTable = CreateTable();
            baseTable.FindColumn("B").AddIndex("IDX_B");
            var baseTables = new List<DatabaseTable> { baseTable };
            var compareTable = CreateTable();
            compareTable.FindColumn("B").AddIndex("IDX_B");
            compareTable.Indexes.Find(i => i.Name == "IDX_B").IsUnique = true;
            var compareTables = new List<DatabaseTable> { compareTable };

            //act
            target.Execute(baseTables, compareTables);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(sb.Count == 1, "1 change");
            Assert.IsTrue(sb.First().ResultType == ResultType.Change, "It is a change, although will always be a drop/add");
            Assert.IsTrue(result.Contains("CREATE UNIQUE NONCLUSTERED INDEX [IDX_B]"), "add the new column");
        }


        [TestMethod]
        public void WhenTableIndexFilterChanged()
        {
            //arrange
            var sb = new List<CompareResult>();
            var writer = new ComparisonWriter(SqlType.SqlServer);
            var target = new CompareTables(sb, writer);

            var baseTable = CreateTable();
            baseTable.FindColumn("B").AddIndex("IDX_B");
            var baseTables = new List<DatabaseTable> { baseTable };
            var compareTable = CreateTable();
            compareTable.FindColumn("B").AddIndex("IDX_B");
            compareTable.Indexes.Find(i => i.Name == "IDX_B").Filter = "(B IS NOT NULL)";
            var compareTables = new List<DatabaseTable> { compareTable };

            //act
            target.Execute(baseTables, compareTables);
            var result = string.Join(Environment.NewLine, sb.Select(x => x.Script).ToArray());

            //assert
            Assert.IsTrue(sb.Count == 1, "1 change");
            Assert.IsTrue(sb.First().ResultType == ResultType.Change, "It is a change, although will always be a drop/add");
            Assert.IsTrue(result.Contains("CREATE NONCLUSTERED INDEX [IDX_B] ON [Test]([B]) WHERE (B IS NOT NULL);"), 
                "add the new column");
        }
    }
}