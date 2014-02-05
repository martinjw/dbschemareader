using System;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
#endif

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

            //act
            var sql = migration.AddConstraint(table, pk);

            //assert
            Assert.IsTrue(sql.IndexOf("ADD CONSTRAINT [Test_PK] PRIMARY KEY NONCLUSTERED ([Id])", StringComparison.OrdinalIgnoreCase) != -1, "adding a primary key");
        }
    }
}
