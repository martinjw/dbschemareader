using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    /// <summary>
    /// Summary description for AccessConversionTest
    /// </summary>
    [TestClass]
    public class AccessConversionTest
    {
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();
        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };

        [TestMethod]
        public void TestLong()
        {
            //arrange
            _column.DbDataType = "LONG";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("INT", result);
        }


        [TestMethod]
        public void TestBoolean()
        {
            //arrange
            _column.DbDataType = "BOOLEAN";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("BIT", result);
        }

        [TestMethod]
        public void TestCurrency()
        {
            //arrange
            _column.DbDataType = "CURRENCY";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("MONEY", result);
        }

        [TestMethod]
        public void TestBinary()
        {
            //arrange
            _column.DbDataType = "BINARY";
            _column.Length = 0;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("VARBINARY (MAX)", result);
        }

        [TestMethod]
        public void TestGuid()
        {
            //arrange
            _column.DbDataType = "GUID";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("UNIQUEIDENTIFIER", result);
        }

        [TestMethod]
        public void TestAccessText()
        {
            //arrange
            _column.DbDataType = "TEXT";
            _column.Length = 5;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NVARCHAR (5)", result);
        }

        [TestMethod]
        public void TestSqlServerText()
        {
            //arrange
            _column.DbDataType = "TEXT";
            _column.Length = null;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TEXT", result);
        }

        [TestMethod]
        public void TestMemo()
        {
            //arrange
            _column.DbDataType = "TEXT"; //MEMO
            _column.Length = 0;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NVARCHAR (MAX)", result);
        }
    }
}
