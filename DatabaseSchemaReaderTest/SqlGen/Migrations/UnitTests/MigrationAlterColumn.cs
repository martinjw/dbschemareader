using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationAlterColumn
    {
        //ALTER column commands may fail if the datatype is not implicitly convertable, or data exceeds the restrictions.
        //we'll take an easy one- increasing the length of a varchar column

        [TestMethod]
        public void TestSqlServerWithSchema()
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
            Assert.IsTrue(sql.Contains("ALTER TABLE [dbo].[Orders] ALTER COLUMN [NAME] VARCHAR (40)"), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqlServerNoSchema()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;

            //act
            migration.IncludeSchema = false;
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE [Orders] ALTER COLUMN [NAME] VARCHAR (40)"), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestOracleWithSchema()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;

            //act
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" MODIFY \"NAME\" NVARCHAR2 (40)"), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestOracleNoSchema()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;

            //act
            migration.IncludeSchema = false;
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"Orders\" MODIFY \"NAME\" NVARCHAR2 (40)"), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestMySqlWithSchema()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;

            //act
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE `dbo`.`Orders` MODIFY `NAME` VARCHAR (40)"), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestMySqlNoSchema()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;

            //act
            migration.IncludeSchema = false;
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE `Orders` MODIFY `NAME` VARCHAR (40)"), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqLite()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;

            //act
            var sql = migration.AlterColumn(table, column, null);

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
            column.Length = 40;

            //act
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" ALTER COLUMN \"NAME\" VARCHAR (40)"), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestPostgreSql()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = table.FindColumn("NAME");
            column.Length = 40;
            column.DefaultValue = "Bob";

            //act
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" ALTER COLUMN \"NAME\" TYPE VARCHAR (40) USING CAST(\"NAME\" AS VARCHAR (40))"), "names should be quoted correctly");
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" ALTER COLUMN \"NAME\" SET DEFAULT 'Bob';"), "default is set");
            Assert.IsTrue(sql.Contains("ALTER TABLE \"dbo\".\"Orders\" ALTER COLUMN \"NAME\" SET NOT NULL;"), "NULL should be handled correctly");
        }

        [TestMethod]
        public void TestPostgreSqlWithFunction()
        {
            //#131
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders")
                .AddColumn<DateTime>("CreationDate").Table;
            var column = table.FindColumn("CreationDate");
            column.DefaultValue = "now()";

            //act
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"Orders\" ALTER COLUMN \"CreationDate\" SET DEFAULT now();"), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestPostgreSqlWithBoolean()
        {
            //#131
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders")
                .AddColumn("IsActive", "bool").Table;
            var column = table.FindColumn("IsActive");
            column.DefaultValue = "TRUE";

            //act
            var sql = migration.AlterColumn(table, column, null);

            //assert
            Assert.IsTrue(sql.Contains("ALTER TABLE \"Orders\" ALTER COLUMN \"IsActive\" SET DEFAULT TRUE;"), "value should be set correctly");
        }
    }
}