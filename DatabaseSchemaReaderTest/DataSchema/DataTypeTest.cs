using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{

    /// <summary>
    /// Test DataType NetCodeName behaviour
    ///</summary>
    [TestClass]
    public class DataTypeTest
    {
        [TestMethod]
        public void StringNetCodeNameTest()
        {
            //arrange
            var column = new DatabaseColumn
            {
                Name = "TEST",
                Length = 20,
                DbDataType = "VARCHAR2",
                DataType = new DataType("VARCHAR2", "System.String")
            };


            //act
            var result = column.DataType.NetCodeName(column);

            //assert
            Assert.AreEqual("string", result);
        }

        [TestMethod]
        public void DecimalTest()
        {
            //arrange
            var column = new DatabaseColumn
            {
                Name = "TEST",
                Precision = 8,
                Scale = 2
            };
            AddNumberDataType(column);

            //act
            var result = column.DataType.NetCodeName(column);

            //assert
            Assert.AreEqual("decimal", result);
        }


        [TestMethod]
        public void IntegerTest()
        {
            //arrange
            var column = new DatabaseColumn
            {
                Name = "TEST",
                Precision = 6,
                Scale = 0
            };
            AddNumberDataType(column);

            //act
            var result = column.DataType.NetCodeName(column);

            //assert
            Assert.AreEqual("int", result);
        }


        [TestMethod]
        public void LongTest()
        {
            //arrange
            var column = new DatabaseColumn
            {
                Name = "TEST",
                Precision = 12,
                Scale = 0
            };
            AddNumberDataType(column);

            //act
            var result = column.DataType.NetCodeName(column);

            //assert
            Assert.AreEqual("long", result);
        }

        [TestMethod]
        public void ShortTest()
        {
            //arrange
            var column = new DatabaseColumn
            {
                Name = "TEST",
                Precision = 4,
                Scale = 0
            };
            AddNumberDataType(column);

            //act
            var result = column.DataType.NetCodeName(column);

            //assert
            Assert.AreEqual("short", result);
        }


        private static void AddNumberDataType(DatabaseColumn column)
        {
            column.DbDataType = "NUMBER";

            column.DataType = new DataType("NUMBER", "System.Decimal");
        }
    }
}
