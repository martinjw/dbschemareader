using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Oracle
{
    [TestClass]
    public class StringDataTypesTest
    {
        /*
        --Character--
        VARCHAR2(size)
        NVARCHAR2(size)
        VARCHAR **Obsolete**
        CHAR(size)
        NCHAR(size)
        --Large--
        CLOB
        NCLOB
         */

        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();

        [TestMethod]
        public void TestStringNVarChar()
        {
            //arrange
            _column.DbDataType = "NVARCHAR";
            _column.Length = 5; //in Oracle by default this is bytes, rather than char length

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NVARCHAR2 (5)", result);
        }

        [TestMethod]
        public void TestStringNVarChar2()
        {
            //arrange
            _column.DbDataType = "NVARCHAR2";
            _column.Length = 5;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NVARCHAR2 (5)", result);
        }

        [TestMethod]
        public void TestStringVarChar()
        {
            //arrange
            _column.DbDataType = "VARCHAR";
            _column.Length = 5;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NVARCHAR2 (5)", result); //NB we've changed to unicode here
        }

        [TestMethod]
        public void TestStringVarChar2()
        {
            //arrange
            _column.DbDataType = "VARCHAR2";
            _column.Length = 5;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("VARCHAR2 (5 CHAR)", result); //should have preserved VARCHAR. Assumes CHAR length
        }

        [TestMethod]
        public void TestStringWithMaxLength()
        {
            //arrange
            _column.DbDataType = "NVARCHAR";
            _column.Length = -1;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("CLOB", result);
        }

        [TestMethod]
        public void TestCharUnicode()
        {
            //arrange
            _column.DbDataType = "NCHAR";
            _column.Length = 5;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NCHAR (5)", result);
        }


        [TestMethod]
        public void TestChar()
        {
            //arrange
            _column.DbDataType = "CHAR";
            _column.Length = 20;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("CHAR (20)", result);
        }

        [TestMethod]
        public void TestClob()
        {
            //arrange
            _column.DbDataType = "CLOB";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("CLOB", result);
        }

        [TestMethod]
        public void TestNText()
        {
            //arrange
            _column.DbDataType = "NTEXT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NCLOB", result);
        }

        [TestMethod]
        public void TestText()
        {
            //arrange
            _column.DbDataType = "TEXT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("CLOB", result);
        }

    }
}
