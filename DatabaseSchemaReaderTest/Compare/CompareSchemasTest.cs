using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Compare
{
    [TestClass]
    public class CompareSchemasTest
    {
        [TestMethod]
        public void GivenIdenticalSchemas()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            DatabaseSchema schema2 = CreateSchema();

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.AreEqual(string.Empty, script);
        }


        [TestMethod]
        public void GivenNewTable()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            DatabaseSchema schema2 = CreateSchema();
            schema2.Tables.Add(CreateProductsTable());

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.IsTrue(script.Contains("CREATE TABLE [Products]"));
        }


        [TestMethod]
        public void GivenDroppedTable()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            schema1.Tables.Add(CreateProductsTable());
            DatabaseSchema schema2 = CreateSchema();

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.IsTrue(script.Contains("DROP TABLE [Products]"));
        }

        [TestMethod]
        public void GivenAddedColumn()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            var productsTable = CreateProductsTable();
            schema1.Tables.Add(productsTable);

            DatabaseSchema schema2 = CreateSchema();
            var productsTable2 = CreateProductsTable();
            schema2.Tables.Add(productsTable2);
            var nameColumn = new DatabaseColumn { Name = "Name", DbDataType = "NVARCHAR", Length = 10, Nullable = false };
            productsTable2.Columns.Add(nameColumn);

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.IsTrue(script.Contains("ADD [Name]"));
        }

        [TestMethod]
        public void GivenDroppedColumn()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            var productsTable = CreateProductsTable();
            schema1.Tables.Add(productsTable);
            var nameColumn = new DatabaseColumn { Name = "Name", DbDataType = "NVARCHAR", Length = 10, Nullable = false };
            productsTable.Columns.Add(nameColumn);

            DatabaseSchema schema2 = CreateSchema();
            var productsTable2 = CreateProductsTable();
            schema2.Tables.Add(productsTable2);

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.IsTrue(script.Contains("DROP COLUMN [Name]"));
        }

        [TestMethod]
        public void GivenAlteredColumn()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            var productsTable = CreateProductsTable();
            schema1.Tables.Add(productsTable);
            var nameColumn = new DatabaseColumn { Name = "Name", DbDataType = "NVARCHAR", Length = 10, Nullable = false };
            productsTable.Columns.Add(nameColumn);

            DatabaseSchema schema2 = CreateSchema();
            var productsTable2 = CreateProductsTable();
            var nameColumn2 = new DatabaseColumn { Name = "Name", DbDataType = "NVARCHAR", Length = 20, Nullable = true };
            productsTable2.Columns.Add(nameColumn2);
            schema2.Tables.Add(productsTable2);

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.IsTrue(script.Contains("ALTER COLUMN [Name]"));
        }

        [TestMethod]
        public void GivenNewUniqueConstraint()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            DatabaseSchema schema2 = CreateSchema();
            DatabaseConstraint constraint = GetUniqueConstraint();
            schema2.Tables[0].AddConstraint(constraint);

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.IsTrue(script.Contains("ADD CONSTRAINT [UK_NAME] UNIQUE"), script);
        }

        public void GivenSameUniqueConstraint()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            DatabaseConstraint constraint = GetUniqueConstraint();
            schema1.Tables[0].AddConstraint(constraint);
            DatabaseSchema schema2 = CreateSchema();
            DatabaseConstraint constraint2 = GetUniqueConstraint();
            schema2.Tables[0].AddConstraint(constraint2);

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.AreEqual(string.Empty, script);
        }

        public void GivenUniqueConstraintWithChangedColumn()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            DatabaseConstraint constraint = GetUniqueConstraint();
            schema1.Tables[0].AddConstraint(constraint);

            DatabaseSchema schema2 = CreateSchema();
            DatabaseConstraint constraint2 = GetUniqueConstraint();
            constraint2.Columns[0] = "Desc";
            schema2.Tables[0].AddConstraint(constraint2);

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.IsTrue(script.Contains("DROP CONSTRAINT [UK_NAME]"), script);
            Assert.IsTrue(script.Contains("ADD CONSTRAINT [UK_NAME] UNIQUE"), script);
        }

        public void GivenDroppedUniqueConstraint()
        {
            //arrange
            DatabaseSchema schema1 = CreateSchema();
            DatabaseConstraint constraint = GetUniqueConstraint();
            schema1.Tables[0].AddConstraint(constraint);
            DatabaseSchema schema2 = CreateSchema();

            //act
            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //assert
            Assert.IsTrue(script.Contains("DROP CONSTRAINT [UK_NAME]"), script);
        }

        private static DatabaseConstraint GetUniqueConstraint()
        {
            var constraint = new DatabaseConstraint
                                 {
                                     Name = "UK_NAME",
                                     ConstraintType = ConstraintType.UniqueKey,
                                 };
            constraint.Columns.Add("Name");
            return constraint;
        }

        private static DatabaseSchema CreateSchema()
        {
            var schema = new DatabaseSchema(null, "System.Data.SqlClient");

            var orderTable = new DatabaseTable { Name = "Orders" };
            schema.Tables.Add(orderTable);

            var idColumn = new DatabaseColumn { Name = "Id", DbDataType = "int" };
            orderTable.Columns.Add(idColumn);

            var nameColumn = new DatabaseColumn { Name = "Name", DbDataType = "VARCHAR" };
            orderTable.Columns.Add(nameColumn);

            var descColumn = new DatabaseColumn { Name = "Desc", DbDataType = "VARCHAR" };
            orderTable.Columns.Add(descColumn);

            var pk = new DatabaseConstraint { ConstraintType = ConstraintType.PrimaryKey, Name = "PK_Orders" };
            pk.Columns.Add("Id");
            orderTable.PrimaryKey = pk;

            return schema;
        }

        private static DatabaseTable CreateProductsTable()
        {
            var productsTable = new DatabaseTable { Name = "Products" };

            var idColumn = new DatabaseColumn { Name = "Id", DbDataType = "int" };
            productsTable.Columns.Add(idColumn);

            var pk = new DatabaseConstraint { ConstraintType = ConstraintType.PrimaryKey, Name = "PK_Orders" };
            pk.Columns.Add("Id");
            productsTable.PrimaryKey = pk;

            return productsTable;
        }
    }
}
