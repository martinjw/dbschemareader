using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Compare
{
    [TestClass]
    public class CompareResultFindTest
    {
        private const string Schema = "dbo";

        private void AssertType<T>(object value)
        {
            Assert.IsInstanceOfType(value, typeof(T));
        }

        [TestMethod]
        public void FindTable()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
                {
                    Name = "Orders",
                    SchemaOwner = Schema,
                    SchemaObjectType = SchemaObjectType.Table
                };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseTable>(find);
        }

        [TestMethod]
        public void FindColumn()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                SchemaOwner = Schema,
                Name = "Name",
                SchemaObjectType = SchemaObjectType.Column
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseColumn>(find);
        }

        [TestMethod]
        public void FindPrimaryKey()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                SchemaOwner = Schema,
                Name = "PK_Orders",
                SchemaObjectType = SchemaObjectType.Constraint
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseConstraint>(find);
        }

        [TestMethod]
        public void FindUniqueConstraint()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                SchemaOwner = Schema,
                Name = "UK_NAME",
                SchemaObjectType = SchemaObjectType.Constraint
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseConstraint>(find);
        }

        [TestMethod]
        public void FindForeignKey()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Order_Line",
                SchemaOwner = Schema,
                Name = "FK_OrderLine_Orders",
                SchemaObjectType = SchemaObjectType.Constraint
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseConstraint>(find);
        }


        [TestMethod]
        public void FindIndex()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                SchemaOwner = Schema,
                Name = "IDX_Desc",
                SchemaObjectType = SchemaObjectType.Index
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseIndex>(find);
        }


        [TestMethod]
        public void FindTrigger()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                TableName = "Orders",
                SchemaOwner = Schema,
                Name = "MyTrigger",
                SchemaObjectType = SchemaObjectType.Trigger
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseTrigger>(find);
        }


        [TestMethod]
        public void FindProcedure()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MySproc",
                SchemaOwner = Schema,
                SchemaObjectType = SchemaObjectType.StoredProcedure
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseStoredProcedure>(find);
        }

        [TestMethod]
        public void FindFunction()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MyFunction",
                SchemaOwner = Schema,
                SchemaObjectType = SchemaObjectType.Function
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseFunction>(find);
        }

        [TestMethod]
        public void FindPackage()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MyPackage",
                SchemaOwner = Schema,
                SchemaObjectType = SchemaObjectType.Package
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabasePackage>(find);
        }

        [TestMethod]
        public void FindView()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MyView",
                SchemaOwner = Schema,
                SchemaObjectType = SchemaObjectType.View
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseView>(find);
        }

        [TestMethod]
        public void FindSequence()
        {
            //arrange
            DatabaseSchema schema = CreateSchema();
            var result = new CompareResult
            {
                Name = "MySequence",
                SchemaOwner = Schema,
                SchemaObjectType = SchemaObjectType.Sequence
            };

            //act
            var find = result.Find(schema);

            //assert
            AssertType<DatabaseSequence>(find);
        }

        private static DatabaseSchema CreateSchema()
        {
            var schema = new DatabaseSchema(null, "System.Data.SqlClient");

            var orders = schema.AddTable("Orders")
                  .AddColumn<int>("Id").AddPrimaryKey("PK_Orders")
                  .AddColumn<string>("Name").AddUniqueKey("UK_NAME")
                  .AddColumn<string>("Desc").AddIndex("IDX_Desc")
                  .Table;
            orders.SchemaOwner = Schema;
            orders.Triggers.Add(new DatabaseTrigger { Name = "MyTrigger" });

            var lines = schema.AddTable("Order_Line")
                  .AddColumn<int>("Id").AddPrimaryKey("PK_OrderLine")
                  .AddColumn<int>("Price")
                  .AddColumn<int>("Order_Id").AddForeignKey("FK_OrderLine_Orders", "Orders")
                  .Table;
            lines.SchemaOwner = Schema;

            schema.StoredProcedures.Add(new DatabaseStoredProcedure { Name = "MySproc", SchemaOwner = Schema });
            schema.Functions.Add(new DatabaseFunction { Name = "MyFunction", SchemaOwner = Schema });
            schema.Packages.Add(new DatabasePackage { Name = "MyPackage", SchemaOwner = Schema });
            schema.Views.Add(new DatabaseView { Name = "MyView", SchemaOwner = Schema });
            schema.Sequences.Add(new DatabaseSequence { Name = "MySequence", SchemaOwner = Schema });

            //table with same name under different schema
            var orders2 = schema.AddTable("Orders")
              .AddColumn<int>("Id").AddPrimaryKey("PK_Orders")
              .AddColumn<string>("Name").AddUniqueKey("UK_NAME")
              .AddColumn<string>("Desc").AddIndex("IDX_Desc")
              .Table;
            orders2.SchemaOwner = Schema + "2";


            return schema;
        }

    }
}
