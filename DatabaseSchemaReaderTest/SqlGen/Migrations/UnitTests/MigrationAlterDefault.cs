using System;
using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationAlterDefault
    {
        [TestMethod]
        public void TestSqlServerWithNewDefault()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;
            column.DefaultValue = "'?'";

            //act
            var sql = migration.AlterColumn(table, column, null);

            //assert
            //ALTER TABLE [dbo].[Orders] ALTER COLUMN [NAME] VARCHAR (40)  NOT NULL;
            //ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [DF_Orders_NAME] DEFAULT '?' FOR [NAME];
            Assert.IsTrue(sql.Contains("ALTER COLUMN [NAME] VARCHAR (40)  NOT NULL"), "alter column should not have DEFAULT");
            Assert.IsTrue(sql.Contains("ADD CONSTRAINT [DF_Orders_NAME] DEFAULT '?' FOR [NAME]"), "add default constraint");
        }

        [TestMethod]
        public void TestSqlServerWithNoDefault()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;

            //act
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsFalse(sql.Contains("ADD CONSTRAINT [DF_Orders_NAME] DEFAULT"), "no add default constraint");
        }

        [TestMethod]
        public void TestSqlServerWithChangingDefault()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;
            column.DefaultValue = "'?'";
            //create an "original" version of the column (by cloning)
            var originalColumn = column.Clone();
            originalColumn.DefaultValue = "'UNKNOWN'";
            //add a default constraint
            var df = new DatabaseConstraint { ConstraintType = ConstraintType.Default, Name = "DF_Orders_Name" };
            df.Columns.Add("NAME");
            table.AddConstraint(df);

            //act
            var sql = migration.AlterColumn(table, column, originalColumn);

            //assert
            //ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF_Orders_Name];
            //-- Orders from [NAME] VARCHAR (40)  NOT NULL to [NAME] VARCHAR (40)  NOT NULL
            //ALTER TABLE [dbo].[Orders] ALTER COLUMN [NAME] VARCHAR (40)  NOT NULL;
            //ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [DF_Orders_Name] DEFAULT '?' FOR [NAME];

            Assert.IsTrue(sql.Contains("DROP CONSTRAINT [DF_Orders_Name]"), "drop constraint");
            Assert.IsTrue(sql.Contains("ADD CONSTRAINT [DF_Orders_Name] DEFAULT '?' FOR [NAME]"), "add default constraint");
        }

        [TestMethod]
        public void TestSqlServerDropColumnWithDefault()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;
            column.DefaultValue = "'?'";
            //add a default constraint
            var df = new DatabaseConstraint { ConstraintType = ConstraintType.Default, Name = "DF_Orders_Name" };
            df.Columns.Add("NAME");
            table.AddConstraint(df);

            //act
            var sql = migration.DropColumn(table, column);

            //assert
            //ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [DF_Orders_Name];
            //ALTER TABLE [dbo].[Orders] DROP COLUMN [NAME];

            Assert.IsTrue(sql.Contains("DROP CONSTRAINT [DF_Orders_Name]"), "drop constraint");
        }

        [TestMethod]
        public void TestSqlServerAddDefaultConstraint()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;
            column.DefaultValue = "'?'";
            //add a default constraint
            var df = new DatabaseConstraint { ConstraintType = ConstraintType.Default, Name = "DF_Orders_Name", Expression = "'?'" };
            df.Columns.Add("NAME");
            table.AddConstraint(df);

            //act
            var sql = migration.AddConstraint(table, df);

            //assert
            //ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [DF_Orders_Name] DEFAULT '?' FOR [NAME];

            Assert.IsTrue(sql.Contains("ADD CONSTRAINT [DF_Orders_Name] DEFAULT '?' FOR [NAME]"), "add constraint");
        }

    }
}
