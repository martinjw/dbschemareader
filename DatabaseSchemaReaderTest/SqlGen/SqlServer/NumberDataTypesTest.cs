using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    [TestClass]
    public class NumberDataTypesTest
    {
        /*
        --Integer-- 
        BIGINT 
        INT, INTEGER 
        SMALLINT 
        TINYINT 
        --Decimal Data--
        DECIMAL(p,s), NUMERIC(p,s) 
        Floating
        FLOAT(p) 
        REAL 
        DOUBLE PRECISION 
        --Other--
        BIT 
        MONEY 
        SMALLMONEY 
          */
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();
        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };

        [TestMethod]
        public void TestInteger()
        {
            //arrange
            _column.DbDataType = "INT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("INT", result);
        }

        [TestMethod]
        public void TestMySqlInteger()
        {
            //arrange
            _column.DbDataType = "int(10) unsigned";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("INT", result);
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
        public void TestMySqlSmallInt()
        {
            //arrange
            _column.DbDataType = "smallint(5) unsigned";

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
        public void TestBit()
        {
            //arrange
            _column.DbDataType = "BIT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("BIT", result);
        }

        [TestMethod]
        public void TestMoney()
        {
            //arrange
            _column.DbDataType = "MONEY";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("MONEY", result);
        }
    }
}
