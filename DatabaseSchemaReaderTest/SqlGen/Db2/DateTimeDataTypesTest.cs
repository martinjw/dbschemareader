using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.Db2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Db2
{
    [TestClass]
    public class DateTimeDataTypesTest
    {
        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();

        [TestMethod]
        public void TestDateTime()
        {
            //arrange
            _column.DbDataType = "DATETIME";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIMESTAMP", result);
        }

        [TestMethod]
        public void TestDateTime2()
        {
            //arrange
            _column.DbDataType = "DATETIME2";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIMESTAMP", result);
        }

        [TestMethod]
        public void TestTimeStamp()
        {
            //arrange
            _column.DbDataType = "TIMESTAMP";
            _column.Precision = 5;
            _column.DataType = null;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIMESTAMP", result);
        }


        [TestMethod]
        public void TestSqlServerTimeStamp()
        {
            //arrange
            _column.DbDataType = "TIMESTAMP";
            _column.Precision = 18;
            _column.DataType = new DataType("TIMESTAMP", "byte[]");
            _column.DataType.ProviderDbType = (int)SqlDbType.Timestamp;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("GRAPHIC", result);
        }

        [TestMethod]
        public void TestDate()
        {
            //arrange
            _column.DbDataType = "DATE";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATE", result);
        }
    }
}
