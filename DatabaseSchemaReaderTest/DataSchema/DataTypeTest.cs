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
            var column = new DatabaseColumn();
            column.Name = "TEST";
            column.Length = 20;
            column.DbDataType = "VARCHAR2";

            column.DataType = new DataType("VARCHAR2", "System.String");

            //act
            var result = column.DataType.NetCodeName(column);

            //assert
            Assert.AreEqual("string", result);
        }

        [TestMethod]
        public void DateTimeNetCodeNameTest()
        {
            //arrange
            var column = new DatabaseColumn();
            column.Name = "TEST";
            column.DbDataType = "TIMESTAMP";

            column.DataType = new DataType("TIMESTAMP", "System.DateTime");

            //act
            var result = column.DataType.NetCodeName(column);

            //assert
            Assert.AreEqual("DateTime", result);
        }

        [TestMethod]
        public void DateTimeOffsetNetCodeNameTest()
        {
            //arrange
            var column = new DatabaseColumn();
            column.Name = "TEST";
            column.DbDataType = "TIMESTAMP(6) WITH TIME ZONE";

            column.DataType = new DataType("TIMESTAMP WITH TIME ZONE", "System.DateTimeOffset");

            //act
            var result = column.DataType.NetCodeName(column);

            //assert
            Assert.AreEqual("DateTimeOffset", result);
        }

        [TestMethod]
        public void DecimalTest()
        {
            //arrange
            var column = new DatabaseColumn();
            column.Name = "TEST";
            column.Precision = 8;
            column.Scale = 2;
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
            var column = new DatabaseColumn();
            column.Name = "TEST";
            column.Precision = 6;
            column.Scale = 0;
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
            var column = new DatabaseColumn();
            column.Name = "TEST";
            column.Precision = 12;
            column.Scale = 0;
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
            var column = new DatabaseColumn();
            column.Name = "TEST";
            column.Precision = 4;
            column.Scale = 0;
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
