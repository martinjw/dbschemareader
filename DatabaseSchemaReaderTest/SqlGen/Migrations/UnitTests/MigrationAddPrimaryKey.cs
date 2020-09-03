using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationAddPrimaryKey
    {

        [TestMethod]
        public void TestAddPrimaryKey()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test")
                .AddColumn<int>("Id")
                .AddColumn<string>("Name")
                .Table;
            var pk = new DatabaseConstraint
            {
                Name = "Test_PK",
                ConstraintType = ConstraintType.PrimaryKey
            };
            pk.Columns.Add("Id");

            //act
            var sql = migration.AddConstraint(table, pk);

            //assert
            Assert.IsTrue(sql.IndexOf("ADD CONSTRAINT [Test_PK] PRIMARY KEY ([Id])", StringComparison.OrdinalIgnoreCase) != -1, "adding a primary key");
        }

        [TestMethod]
        public void TestAddPrimaryKeyNoEscapeNames()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();
            migration.EscapeNames = false;

            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test")
                .AddColumn<int>("Id")
                .AddColumn<string>("Name")
                .Table;
            var pk = new DatabaseConstraint
            {
                Name = "Test_PK",
                ConstraintType = ConstraintType.PrimaryKey
            };
            pk.Columns.Add("Id");

            //act
            var sql = migration.AddConstraint(table, pk);

            //assert
            Assert.IsTrue(sql.IndexOf("ADD CONSTRAINT Test_PK PRIMARY KEY (Id)", StringComparison.OrdinalIgnoreCase) != -1, "adding a primary key");
        }

        [TestMethod]
        public void TestAddPrimaryKeyWithGuid()
        {

            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            var table = schema.AddTable("Test")
                .AddColumn<Guid>("Id")
                .AddColumn<string>("Name")
                .Table;
            var pk = new DatabaseConstraint
            {
                Name = "Test_PK",
                ConstraintType = ConstraintType.PrimaryKey
            };
            pk.Columns.Add("Id");
            table.Indexes.Add(new DatabaseIndex
            {
                IndexType = "PRIMARY NONCLUSTERED",
                Columns = { new DatabaseColumn { Name = "Id" } }
            });

            //act
            var sql = migration.AddConstraint(table, pk);

            //assert
            Assert.IsTrue(sql.IndexOf("ADD CONSTRAINT [Test_PK] PRIMARY KEY NONCLUSTERED([Id])", StringComparison.OrdinalIgnoreCase) != -1, "adding a primary key");
        }
    }
}
