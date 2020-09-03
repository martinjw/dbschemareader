using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationAddColumn
    {

        public static DatabaseColumn CreateNewColumn()
        {
            return new DatabaseColumn
            {
                Name = "COUNTRY",
                DbDataType = "VARCHAR",
                Length = 20,
                DataType = new DataType("VARCHAR", "string"),
                Nullable = true
            };
        }

        [TestMethod]
        public void TestSqlServerWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE [dbo].[Orders] ADD [COUNTRY] VARCHAR (20)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqlServerNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE [Orders] ADD [COUNTRY] VARCHAR (20)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqlServerNoEscapeNames()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();
            migration.EscapeNames = false;

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE Orders ADD COUNTRY VARCHAR (20)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestOracleWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"dbo\".\"Orders\" ADD \"COUNTRY\" NVARCHAR2 (20)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestOracleNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"Orders\" ADD \"COUNTRY\" NVARCHAR2 (20)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestMySqlWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE `dbo`.`Orders` ADD `COUNTRY` VARCHAR (20)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestMySqlNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE `Orders` ADD `COUNTRY` VARCHAR (20)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqLite()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE [Orders] ADD [COUNTRY] TEXT;", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestDb2()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Db2).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"dbo\".\"Orders\" ADD \"COUNTRY\" VARCHAR (20)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestPostgreSql()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = CreateNewColumn();

            //act
            var sql = migration.AddColumn(table, column);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"dbo\".\"Orders\" ADD \"COUNTRY\" VARCHAR (20)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }
    }
}
