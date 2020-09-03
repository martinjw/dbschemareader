using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationDropColumn
    {

        [TestMethod]
        public void TestSqlServerWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE [dbo].[Orders] DROP COLUMN [COUNTRY]", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqlServerNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            migration.IncludeSchema = false;
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE [Orders] DROP COLUMN [COUNTRY]", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqlServerNoSchemaNoEscapeNames()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();
            migration.EscapeNames = false;

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            migration.IncludeSchema = false;
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE Orders DROP COLUMN COUNTRY", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestOracleWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"dbo\".\"Orders\" DROP COLUMN \"COUNTRY\"", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestOracleNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            migration.IncludeSchema = false;
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"Orders\" DROP COLUMN \"COUNTRY\"", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestMySqlWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE `dbo`.`Orders` DROP COLUMN `COUNTRY`", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestMySqlNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            migration.IncludeSchema = false;
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE `Orders` DROP COLUMN `COUNTRY`", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqLite()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("--", StringComparison.OrdinalIgnoreCase), "Not available in SQLite");
        }


        [TestMethod]
        public void TestDb2()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Db2).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"dbo\".\"Orders\" DROP COLUMN \"COUNTRY\"", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestPostgreSql()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();

            //act
            var sql = migration.DropColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"dbo\".\"Orders\" DROP COLUMN \"COUNTRY\" CASCADE", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }
    }
}
