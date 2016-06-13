using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationDropIndex
    {

        [TestMethod]
        public void TestSqlServerWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            var sql = migration.DropIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("DROP INDEX [UI_COUNTRY] ON [dbo].[Orders]", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqlServerNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            migration.IncludeSchema = false;
            var sql = migration.DropIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("DROP INDEX [UI_COUNTRY] ON [Orders]", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestOracleWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            var sql = migration.DropIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("DROP INDEX \"UI_COUNTRY\"", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestOracleNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            migration.IncludeSchema = false;
            var sql = migration.DropIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("DROP INDEX \"UI_COUNTRY\"", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestMySqlWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            var sql = migration.DropIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("DROP INDEX `UI_COUNTRY` ON `dbo`.`Orders`", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestMySqlNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            migration.IncludeSchema = false;
            var sql = migration.DropIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("DROP INDEX `UI_COUNTRY` ON `Orders`", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqLite()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            var sql = migration.DropIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("DROP INDEX [UI_COUNTRY]", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestDb2()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Db2).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            var sql = migration.DropIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("DROP INDEX \"UI_COUNTRY\"", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestPostgreSql()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            var sql = migration.DropIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("DROP INDEX IF EXISTS \"UI_COUNTRY\" CASCADE", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }
    }
}
