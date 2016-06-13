using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    [TestClass]
    public class StringDataTypesTest
    {
        /*
        --Character--
        CHAR(n) 
        NCHAR(n) 
        VARCHAR(n | max) 
        NVARCHAR(n | max) 
        TEXT 
        NTEXT 
         */
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();
        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };

        [TestMethod]
        public void TestStringNVarChar()
        {
            //arrange
            _column.DbDataType = "NVARCHAR";
            _column.Length = 5;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NVARCHAR (5)", result);
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
            Assert.AreEqual("NVARCHAR (5)", result);
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
            Assert.AreEqual("VARCHAR (5)", result); //NB we've changed to unicode here
        }

        [TestMethod]
        public void TestMySqlStringVarChar()
        {
            //arrange
            _column.DbDataType = "varchar(5)";
            _column.Length = 5;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("VARCHAR (5)", result); //NB we've changed to unicode here
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
            Assert.AreEqual("VARCHAR (5)", result);
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
            Assert.AreEqual("NVARCHAR (MAX)", result);
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
            Assert.AreEqual("NVARCHAR (MAX)", result);
        }

        [TestMethod]
        public void TestNText()
        {
            //arrange
            _column.DbDataType = "NTEXT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NTEXT", result);
        }

        [TestMethod]
        public void TestText()
        {
            //arrange
            _column.DbDataType = "TEXT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TEXT", result);
        }

        [TestMethod]
        public void TestTextWithMaxLength()
        {
            //arrange
            _column.DbDataType = "TEXT";
            _column.Length = int.MaxValue;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TEXT", result);
        }

        [TestMethod]
        public void TestTextWithSpecificLength()
        {
            //arrange
            _column.DbDataType = "TEXT";
            _column.Length = 200;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NVARCHAR (200)", result);
        }

    }
}
