using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationAddForeignKey
    {

        [TestMethod]
        public void TestSqlServerWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE [dbo].[Orders] ADD CONSTRAINT [FK_Orders] FOREIGN KEY ([Parent]) REFERENCES [dbo].[Orders] ([Id])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqlServerNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders] FOREIGN KEY ([Parent]) REFERENCES [Orders] ([Id])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqlServerNoEscapeNames()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();
            migration.EscapeNames = false;

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE Orders ADD CONSTRAINT FK_Orders FOREIGN KEY (Parent) REFERENCES Orders (Id)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestOracleWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"dbo\".\"Orders\" ADD CONSTRAINT \"FK_Orders\" FOREIGN KEY (\"Parent\") REFERENCES \"dbo\".\"Orders\" (\"Id\")", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestOracleNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"Orders\" ADD CONSTRAINT \"FK_Orders\" FOREIGN KEY (\"Parent\") REFERENCES \"Orders\" (\"Id\")", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestOracleNoSchemaNoEscapeNames()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();
            migration.EscapeNames = false;

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE Orders ADD CONSTRAINT FK_Orders FOREIGN KEY (Parent) REFERENCES Orders (Id)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestMySqlWithSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE `dbo`.`Orders` ADD CONSTRAINT `FK_Orders` FOREIGN KEY (`Parent`) REFERENCES `dbo`.`Orders` (`Id`)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestMySqlNoSchema()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE `Orders` ADD CONSTRAINT `FK_Orders` FOREIGN KEY (`Parent`) REFERENCES `Orders` (`Id`)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqLite()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SQLite).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(string.IsNullOrEmpty(sql), "SQLite does not support added constraints after table creation");
        }


        [TestMethod]
        public void TestDb2()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Db2).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"dbo\".\"Orders\" ADD CONSTRAINT \"FK_Orders\" FOREIGN KEY (\"Parent\") REFERENCES \"dbo\".\"Orders\" (\"Id\")", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestPostgreSql()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var fk = MigrationCommon.CreateForeignKey(table);

            //act
            var sql = migration.AddConstraint(table, fk);

            //assert
            Assert.IsTrue(sql.StartsWith("ALTER TABLE \"dbo\".\"Orders\" ADD CONSTRAINT \"FK_Orders\" FOREIGN KEY (\"Parent\") REFERENCES \"dbo\".\"Orders\" (\"Id\")", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }
    }
}
