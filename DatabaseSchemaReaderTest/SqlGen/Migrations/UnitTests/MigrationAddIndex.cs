using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationAddIndex
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
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX [UI_COUNTRY] ON [dbo].[Orders]([COUNTRY])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqlServerNonUnique()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");
            index.IsUnique = false;

            //act
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE INDEX [UI_COUNTRY] ON [dbo].[Orders]([COUNTRY])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqlServerNonUnique_NonClustered()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");
            index.IsUnique = false;
            index.IndexType = "NONCLUSTERED";

            //act
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE NONCLUSTERED INDEX [UI_COUNTRY] ON [dbo].[Orders]([COUNTRY])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }


        [TestMethod]
        public void TestSqlServerNonUnique_Clustered()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");
            index.IsUnique = false;
            index.IndexType = "CLUSTERED";

            //act
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE CLUSTERED INDEX [UI_COUNTRY] ON [dbo].[Orders]([COUNTRY])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
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
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX [UI_COUNTRY] ON [Orders]([COUNTRY])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
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
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX UI_COUNTRY ON Orders(COUNTRY)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqlServerNoSchema_NonClustered()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");
            index.IndexType = "NONCLUSTERED";

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE NONCLUSTERED INDEX [UI_COUNTRY] ON [Orders]([COUNTRY])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestSqlServerNoSchema_Clustered()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");
            index.IndexType = "CLUSTERED";

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE CLUSTERED INDEX [UI_COUNTRY] ON [Orders]([COUNTRY])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
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
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX \"UI_COUNTRY\" ON \"dbo\".\"Orders\"(\"COUNTRY\")", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
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
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX \"UI_COUNTRY\" ON \"Orders\"(\"COUNTRY\")", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestOracleNotUnique()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.Oracle).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");
            index.IsUnique = false;

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE INDEX \"UI_COUNTRY\" ON \"Orders\"(\"COUNTRY\")", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
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
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX `UI_COUNTRY` ON `dbo`.`Orders`(`COUNTRY`)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
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
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX `UI_COUNTRY` ON `Orders`(`COUNTRY`)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }

        [TestMethod]
        public void TestMySqlNotUnique()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.MySql).MigrationGenerator();

            var table = MigrationCommon.CreateTestTable("Orders");
            table.SchemaOwner = "dbo";
            var column = MigrationCommon.CreateNewColumn();
            var index = MigrationCommon.CreateUniqueIndex(column, "COUNTRY");
            index.IsUnique = false;

            //act
            migration.IncludeSchema = false;
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE INDEX `UI_COUNTRY` ON `Orders`(`COUNTRY`)", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
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
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX [UI_COUNTRY] ON [Orders]([COUNTRY])", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
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
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX \"UI_COUNTRY\" ON \"dbo\".\"Orders\"(\"COUNTRY\")", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
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
            var sql = migration.AddIndex(table, index);

            //assert
            Assert.IsTrue(sql.StartsWith("CREATE UNIQUE INDEX \"UI_COUNTRY\" ON \"dbo\".\"Orders\"(\"COUNTRY\")", StringComparison.OrdinalIgnoreCase), "names should be quoted correctly");
        }
    }
}
