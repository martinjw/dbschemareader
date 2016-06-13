using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.PostgreSql
{
    [TestClass]
    public class StringDataTypesTest
    {
        /*
character varying [ (n) ]	varchar [ (n) ]	variable-length character string
character [ (n) ]	char [ (n) ]	fixed-length character string
text	 	variable-length character string
         */

        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();
        private readonly DatabaseColumn _column = new DatabaseColumn();

        [TestMethod]
        public void TestStringType1()
        {
            //arrange
            _column.DbDataType = "NVARCHAR";
            _column.Length = 0;
            
            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("VARCHAR", result);
        }

        [TestMethod]
        public void TestStringType2()
        {
            //arrange
            _column.DbDataType = "NVARCHAR2";
            _column.Length = 0;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("VARCHAR", result);
        }

        [TestMethod]
        public void TestStringWithLength()
        {
            //arrange
            _column.DbDataType = "NVARCHAR2";
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
            Assert.AreEqual("TEXT", result);
        }

        [TestMethod]
        public void TestChar()
        {
            //arrange
            _column.DbDataType = "NCHAR";
            _column.Length = -1;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("CHAR", result);
        }


        [TestMethod]
        public void TestCharWithLength()
        {
            //arrange
            _column.DbDataType = "NCHAR";
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
            Assert.AreEqual("TEXT", result);
        }

        [TestMethod]
        public void TestNText()
        {
            //arrange
            _column.DbDataType = "NTEXT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TEXT", result);
        }

    }
}
