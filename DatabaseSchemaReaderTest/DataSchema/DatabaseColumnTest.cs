using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class DatabaseColumnTest
    {
        [TestMethod]
        public void TestDataTypeDefinitionForInt()
        {
            //arrange
            var column = new DatabaseColumn { DbDataType = "int", DataType = new DataType("int", "int") };

            //act
            var result = column.DataTypeDefinition();

            //assert
            Assert.AreEqual("int", result);
        }

        [TestMethod]
        public void TestDataTypeDefinitionForDecimal()
        {
            //arrange
            var column = new DatabaseColumn
                         {
                             DbDataType = "decimal",
                             DataType = new DataType("decimal", "System.Decimal"),
                             Precision = 10,
                             Scale = 2
                         };

            //act
            var result = column.DataTypeDefinition();

            //assert
            Assert.AreEqual("decimal(10,2)", result);
        }

        [TestMethod]
        public void TestDataTypeDefinitionForVarChar()
        {
            //arrange
            var column = new DatabaseColumn
                         {
                             DbDataType = "nvarchar",
                             DataType = new DataType("nvarchar", "System.String"),
                             Length = 15
                         };

            //act
            var result = column.DataTypeDefinition();

            //assert
            Assert.AreEqual("nvarchar(15)", result);
        }

        [TestMethod]
        public void TestDataTypeDefinitionForMySqlVarChar()
        {
            //arrange
            var column = new DatabaseColumn
                         {
                             DbDataType = "varchar(50)",
                             //below will be ignored as MySql gives the full native type
                             DataType = new DataType("VARCHAR", "System.String"),
                             Length = 50
                         };

            //act
            var result = column.DataTypeDefinition();

            //assert
            Assert.AreEqual("varchar(50)", result);
        }


        [TestMethod]
        public void TestDataTypeDefinitionForMySqlSmallInt()
        {
            //arrange
            var column = new DatabaseColumn
                         {
                             DbDataType = "smallint(5) unsigned",
                             //below will be ignored as MySql gives the full native type
                             DataType = new DataType("SMALLINT", "System.Int16"),
                             Precision = 5,
                             Scale = 0
                         };

            //act
            var result = column.DataTypeDefinition();

            //assert
            Assert.AreEqual("smallint(5) unsigned", result);
        }

        [TestMethod]
        public void TestNetDataTypeForString()
        {
            //arrange
            var column = new DatabaseColumn
            {
                DbDataType = "VARCHAR2",
                DataType = new DataType("VARCHAR2", "System.String"),
            };

            //act
            var result = column.NetDataType();

            //assert
            Assert.AreEqual(typeof(string), result);
        }

        [TestMethod]
        public void TestNetDataTypeForDecimal()
        {
            //arrange
            var column = new DatabaseColumn
            {
                DbDataType = "NUMBER",
                DataType = new DataType("NUMBER", "System.Decimal"),
                Precision = 5,
                Scale = 2
            };

            //act
            var result = column.NetDataType();

            //assert
            Assert.AreEqual(typeof(decimal), result);
        }

        [TestMethod]
        public void TestNetDataTypeForInt()
        {
            //arrange
            var column = new DatabaseColumn
            {
                DbDataType = "NUMBER",
                DataType = new DataType("NUMBER", "System.Decimal"),
                Precision = 5,
                Scale = 0
            };

            //act
            var result = column.NetDataType();

            //assert
            Assert.AreEqual(typeof(int), result);
        }

        [TestMethod]
        public void TestNetDataTypeForShort()
        {
            //arrange
            var column = new DatabaseColumn
            {
                DbDataType = "NUMBER",
                DataType = new DataType("NUMBER", "System.Decimal"),
                Precision = 2,
                Scale = 0
            };

            //act
            var result = column.NetDataType();

            //assert
            Assert.AreEqual(typeof(short), result);
        }

        [TestMethod]
        public void TestNetDataTypeForLong()
        {
            //arrange
            var column = new DatabaseColumn
            {
                DbDataType = "NUMBER",
                DataType = new DataType("NUMBER", "System.Decimal"),
                Precision = 12,
                Scale = 0
            };

            //act
            var result = column.NetDataType();

            //assert
            Assert.AreEqual(typeof(long), result);
        }
    }
}
