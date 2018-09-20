using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationRenameTable
    {

        [TestMethod]
        public void TestSqlServerWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            table.Name = "NewOrders";

            //act
            var sql = migration.RenameTable(table, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("sp_rename '[dbo].[OldName]', 'NewOrders'"), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqlServerNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            table.Name = "NewOrders";

            //act
            migration.IncludeSchema = false;
            var sql = migration.RenameTable(table, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("sp_rename '[OldName]', 'NewOrders'"), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestOracleWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            table.Name = "NewOrders";

            //act
            var sql = migration.RenameTable(table, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"OldName\" RENAME TO \"NewOrders\""), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestOracleNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            table.Name = "NewOrders";

            //act
            migration.IncludeSchema = false;
            var sql = migration.RenameTable(table, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"OldName\" RENAME TO \"NewOrders\""), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestMySqlWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            table.Name = "NewOrders";

            //act
            var sql = migration.RenameTable(table, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("RENAME TABLE `dbo`.`OldName` TO `NewOrders`"), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestMySqlNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            table.Name = "NewOrders";

            //act
            migration.IncludeSchema = false;
            var sql = migration.RenameTable(table, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("RENAME TABLE `OldName` TO `NewOrders`"), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqLite()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            table.Name = "NewOrders";

            //act
            var sql = migration.RenameTable(table, "OldName");

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE [OldName] RENAME TO [NewOrders]", StringComparison.OrdinalIgnoreCase), "Sqlite rename");
        }


        [TestMethod]
        public void TestDb2()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Db2).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            table.Name = "NewOrders";

            //act
            var sql = migration.RenameTable(table, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("RENAME TABLE \"dbo\".\"OldName\" TO \"NewOrders\""), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestPostgreSql()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            table.Name = "NewOrders";

            //act
            var sql = migration.RenameTable(table, "OldName");

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"OldName\" RENAME TO \"NewOrders\""), "names should be quoted correctly");
        }
    }
}
