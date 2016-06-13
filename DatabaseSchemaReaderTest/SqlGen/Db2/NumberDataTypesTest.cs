using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.Db2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Db2
{
    [TestClass]
    public class NumberDataTypesTest
    {
        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();

        [TestMethod]
        public void TestInteger()
        {
            //arrange
            _column.DbDataType = "INT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("INTEGER", result);
        }

        [TestMethod]
        public void TestSmallInt()
        {
            //arrange
            _column.DbDataType = "SMALLINT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("SMALLINT", result);
        }

        [TestMethod]
        public void TestNumber()
        {
            //arrange
            _column.DbDataType = "NUMBER";
            _column.Precision = 10;
            _column.Scale = 2;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NUMERIC (10,2)", result);
        }

        [TestMethod]
        public void TestNumeric()
        {
            //arrange
            _column.DbDataType = "NUMERIC";
            _column.Precision = 10;
            _column.Scale = 2;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NUMERIC (10,2)", result);
        }

        [TestMethod]
        public void TestDecimal()
        {
            //arrange
            _column.DbDataType = "DECIMAL";
            _column.Precision = 10;
            _column.Scale = 2;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DECIMAL (10,2)", result);
        }

        [TestMethod]
        public void TestDec()
        {
            //arrange
            _column.DbDataType = "DEC";
            _column.Precision = 10;
            _column.Scale = 2;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DECIMAL (10,2)", result);
        }

        [TestMethod]
        public void TestBit()
        {
            //arrange
            _column.DbDataType = "BIT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("SMALLINT", result);
        }

        [TestMethod]
        public void TestMoney()
        {
            //arrange
            _column.DbDataType = "MONEY";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DECIMAL(19,4)", result);
        }
    }
}
