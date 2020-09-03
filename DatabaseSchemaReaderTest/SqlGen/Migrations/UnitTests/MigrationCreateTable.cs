using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationCreateTable
    {

        [TestMethod]
        public void TestSqlServerCreateTableWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE [dbo].[Orders]", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
            Assert.IsTrue(sql.Contains("ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])"), "Primary key should be set with name");
        }


        [TestMethod]
        public void TestSqlServerCreateTableNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE [Orders]", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
            Assert.IsTrue(sql.Contains("ALTER TABLE [Orders] ADD CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])"), "Primary key should be set with name");
        }

        [TestMethod]
        public void TestSqlServerCreateTableNoSchemaNoEscapeNames()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();
            migration.EscapeNames = false;

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE Orders", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
            Assert.IsTrue(sql.Contains("ALTER TABLE Orders ADD CONSTRAINT PK_Orders PRIMARY KEY (Id)"), "Primary key should be set with name");
        }

        [TestMethod]
        public void TestOracleCreateTableWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE \"dbo\".\"Orders\"", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" ADD CONSTRAINT \"PK_Orders\" PRIMARY KEY (\"Id\")"), "Primary key should be set with name");
        }


        [TestMethod]
        public void TestOracleCreateTableNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE \"Orders\"", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
            Assert.IsTrue(sql.Contains("ALTER TABLE \"Orders\" ADD CONSTRAINT \"PK_Orders\" PRIMARY KEY (\"Id\")"), "Primary key should be set with name");
        }

        [TestMethod]
        public void TestMySqlCreateTableWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE `dbo`.`Orders`", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
            Assert.IsTrue(sql.Contains("`Id` INT NOT NULL PRIMARY KEY,"), "In MySQL we don't set the primary key with a name, because it seems to be rarely done");
        }


        [TestMethod]
        public void TestMySqlCreateTableNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE `Orders`", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
        }


        [TestMethod]
        public void TestSqLiteCreateTable()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            //migration.IncludeSchema = false; //By default, include schema should be false
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE [Orders]", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
            Assert.IsTrue(sql.Contains("[Id] INTEGER PRIMARY KEY NOT NULL"), "Primary key is set without name");
        }


        [TestMethod]
        public void TestDb2()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Db2).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE \"dbo\".\"Orders\"", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
            Assert.IsTrue(sql.Contains("\"Id\" INTEGER NOT NULL PRIMARY KEY"), "Primary key is not set with name");
        }

        [TestMethod]
        public void TestPostgreSql()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";

            //act
            var sql = migration.AddTable(table);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE TABLE \"dbo\".\"Orders\"", StringComparison.OrdinalIgnoreCase), "table name should be quoted correctly");
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" ADD CONSTRAINT \"PK_Orders\" PRIMARY KEY (\"Id\")"), "Primary key is set with name");
        }
    }
}
