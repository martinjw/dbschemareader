using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
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

namespace DatabaseSchemaReaderTest.Compare
{
    [TestClass]
    public class CompareResultFindTest
    {
        [TestMethod]
        public void FindTable()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
                {
                    Name = "Orders",
                    SchemaObjectType = SchemaObjectType.Table
                };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseTable));
        }

        [TestMethod]
        public void FindColumn()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                Name = "Name",
                SchemaObjectType = SchemaObjectType.Column
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseColumn));
        }

        [TestMethod]
        public void FindPrimaryKey()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                Name = "PK_Orders",
                SchemaObjectType = SchemaObjectType.Constraint
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseConstraint));
        }

        [TestMethod]
        public void FindUniqueConstraint()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                Name = "UK_NAME",
                SchemaObjectType = SchemaObjectType.Constraint
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseConstraint));
        }

        [TestMethod]
        public void FindForeignKey()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Order_Line",
                Name = "FK_OrderLine_Orders",
                SchemaObjectType = SchemaObjectType.Constraint
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseConstraint));
        }


        [TestMethod]
        public void FindIndex()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                Name = "IDX_Desc",
                SchemaObjectType = SchemaObjectType.Index
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseIndex));
        }


        [TestMethod]
        public void FindTrigger()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                Name = "MyTrigger",
                SchemaObjectType = SchemaObjectType.Trigger
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseTrigger));
        }


        [TestMethod]
        public void FindProcedure()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MySproc",
                SchemaObjectType = SchemaObjectType.StoredProcedure
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseStoredProcedure));
        }

        [TestMethod]
        public void FindFunction()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MyFunction",
                SchemaObjectType = SchemaObjectType.Function
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseFunction));
        }

        [TestMethod]
        public void FindPackage()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MyPackage",
                SchemaObjectType = SchemaObjectType.Package
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabasePackage));
        }

        [TestMethod]
        public void FindView()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MyView",
                SchemaObjectType = SchemaObjectType.View
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseView));
        }

        [TestMethod]
        public void FindSequence()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MySequence",
                SchemaObjectType = SchemaObjectType.Sequence
            };

            //act
            var find = result.Find(schema);

            //assert
            Assert.IsInstanceOfType(find, typeof(DatabaseSequence));
        }

        private static DatabaseSchema CreateSchema()
        {
            var schema = new DatabaseSchema(null, "System.Data.SqlClient");

            var orders = schema.AddTable("Orders")
                  .AddColumn<int>("Id").AddPrimaryKey("PK_Orders")
                  .AddColumn<string>("Name").AddUniqueKey("UK_NAME")
                  .AddColumn<string>("Desc").AddIndex("IDX_Desc")
                  .Table;
            orders.Triggers.Add(new DatabaseTrigger { Name = "MyTrigger" });

            schema.AddTable("Order_Line")
                  .AddColumn<int>("Id").AddPrimaryKey("PK_OrderLine")
                  .AddColumn<int>("Price")
                  .AddColumn<int>("Order_Id").AddForeignKey("FK_OrderLine_Orders", "Orders");

            schema.StoredProcedures.Add(new DatabaseStoredProcedure { Name = "MySproc" });
            schema.Functions.Add(new DatabaseFunction { Name = "MyFunction" });
            schema.Packages.Add(new DatabasePackage { Name = "MyPackage" });
            schema.Views.Add(new DatabaseView { Name = "MyView" });
            schema.Sequences.Add(new DatabaseSequence { Name = "MySequence" });

            return schema;
        }

    }
}
