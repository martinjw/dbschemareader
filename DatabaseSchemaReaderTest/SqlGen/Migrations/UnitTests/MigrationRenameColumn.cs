using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationRenameColumn
    {

        [TestMethod]
        public void TestSqlServerWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Name = "NEWNAME";

            //act
            var sql = migration.RenameColumn(table, column, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("sp_rename '[dbo].[Orders].[OldName]', 'NEWNAME', 'COLUMN'"), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqlServerNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Name = "NEWNAME";

            //act
            migration.IncludeSchema = false;
            var sql = migration.RenameColumn(table, column, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("sp_rename '[Orders].[OldName]', 'NEWNAME', 'COLUMN'"), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestOracleWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Name = "NEWNAME";

            //act
            var sql = migration.RenameColumn(table, column, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" RENAME COLUMN \"OldName\" TO \"NEWNAME\""), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestOracleNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Name = "NEWNAME";

            //act
            migration.IncludeSchema = false;
            var sql = migration.RenameColumn(table, column, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"Orders\" RENAME COLUMN \"OldName\" TO \"NEWNAME\""), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestMySqlWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Name = "NEWNAME";

            //act
            var sql = migration.RenameColumn(table, column, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE `dbo`.`Orders` CHANGE `OldName` `NEWNAME` VARCHAR (10) NOT NULL"), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestMySqlNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Name = "NEWNAME";

            //act
            migration.IncludeSchema = false;
            var sql = migration.RenameColumn(table, column, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE `Orders` CHANGE `OldName` `NEWNAME` VARCHAR (10) NOT NULL"), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqLite()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Name = "NEWNAME";

            //act
            var sql = migration.RenameColumn(table, column, "OldName");

            //assert
            Assert.IsTrue(sql.StartsWith("--", StringComparison.OrdinalIgnoreCase), "Cannot be changed in SQLite");
        }


        [TestMethod]
        public void TestDb2()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Db2).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Name = "NEWNAME";

            //act
            var sql = migration.RenameColumn(table, column, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" RENAME COLUMN \"OldName\" TO \"NEWNAME\""), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestPostgreSql()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Name = "NEWNAME";

            //act
            var sql = migration.RenameColumn(table, column, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" RENAME COLUMN \"OldName\" TO \"NEWNAME\""), "names should be quoted correctly");
        }
    }
}
