using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Oracle
{
    [TestClass]
    public class NumberDataTypesTest
    {
        /*
        --Numbers--
        NUMBER(p,s)
        PLS_INTEGER
        BINARY_INTEGER
        LONG
         */

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
            Assert.AreEqual("NUMBER (9)", result);
        }

        [TestMethod]
        public void TestMySqlInteger()
        {
            //arrange
            _column.DbDataType = "int(9) unsigned";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NUMBER (9)", result);
        }

        [TestMethod]
        public void TestSmallInt()
        {
            //arrange
            _column.DbDataType = "SMALLINT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NUMBER (5)", result);
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
            Assert.AreEqual("NUMBER (10,2)", result);
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
            Assert.AreEqual("NUMBER (10,2)", result);
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
            Assert.AreEqual("NUMBER (10,2)", result);
        }

        [TestMethod]
        public void TestBit()
        {
            //arrange
            _column.DbDataType = "BIT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NUMBER (1)", result);
        }

        [TestMethod]
        public void TestMoney()
        {
            //arrange
            _column.DbDataType = "MONEY";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NUMBER (15,4)", result);
        }

        [TestMethod]
        public void TestNullableNumberWithDefault()
        {
            //arrange
            var column = new DatabaseColumn
            {
                Nullable = false,
                DbDataType = "NUMBER",
                Precision = 10,
                DefaultValue = "1"
            };

            //act
            var result = _typeWriter.WriteDataType(column);

            //assert
            Assert.AreEqual("NUMBER (10) DEFAULT 1 NOT NULL", result);
        }
    }
}
