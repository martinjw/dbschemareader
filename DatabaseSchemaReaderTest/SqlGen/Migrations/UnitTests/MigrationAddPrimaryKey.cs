using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Linq;

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

        [TestMethod]
        public void TestConstraintEscaping()
        {
            //#119 escaping was not cascaded down from table generator to constraint generator

            //arrange
            var type = SqlType.PostgreSql;

            var schemaTemp = new DatabaseSchema(null, type);
            var newTable = schemaTemp.AddTable("Chapter");
            var idColumn = newTable.AddColumn("Id", DbType.Int64);
            idColumn.AddPrimaryKey("Chapter_Pk");

            var migration = new DdlGeneratorFactory(type).MigrationGenerator();
            migration.EscapeNames = false;

            //act
            var sql = migration.AddTable(newTable);

            Assert.IsTrue(sql.Contains("ALTER TABLE Chapter ADD CONSTRAINT Chapter_Pk PRIMARY KEY (Id)"), "No escaping for names");

            migration.EscapeNames = true;
            sql = migration.AddTable(newTable);
            Assert.IsTrue(sql.Contains("ALTER TABLE \"Chapter\" ADD CONSTRAINT \"Chapter_Pk\" PRIMARY KEY (\"Id\");"), "Escaping for names");
        }


        [TestMethod]
        public void TestPrimaryKeyNoIndexType()
        {
            //#119 escaping was not cascaded down from table generator to constraint generator

            //arrange
            var schemaTemp = new DatabaseSchema(null, SqlType.Oracle);
            var newTable = schemaTemp.AddTable("Chapter");
            var idColumn = newTable.AddColumn("Id", DbType.Int64);
            idColumn.AddPrimaryKey("Chapter_Pk").AddIndex("PK_Index");
            //deliberately set indextype to null (from non SqlServer db)
            newTable.Indexes.Find(x => x.Name == "PK_Index").IndexType = null;

            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();
            migration.EscapeNames = false;

            //act
            var sql = migration.AddTable(newTable);

            Assert.IsTrue(sql.Contains("ALTER TABLE Chapter ADD CONSTRAINT Chapter_Pk PRIMARY KEY (Id)"), "No escaping for names");
        }
    }
}