using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.Db2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Db2
{
    [TestClass]
    public class DataTypesTest
    {
        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();


        [TestMethod]
        public void TestGraphic()
        {
            //arrange
            _column.DbDataType = "GRAPHIC";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("GRAPHIC", result);
        }

        [TestMethod]
        public void TestVarGraphic()
        {
            //arrange
            _column.DbDataType = "VARGRAPHIC";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("VARGRAPHIC", result);
        }

        [TestMethod]
        public void TestLongGraphic()
        {
            //arrange
            _column.DbDataType = "LONG VARGRAPHIC";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("LONG VARGRAPHIC", result);
        }

        [TestMethod]
        public void TestBlob()
        {
            //arrange
            _column.DbDataType = "BLOB";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("BLOB", result);
        }

        [TestMethod]
        public void TestVarBinary()
        {
            //arrange
            _column.DbDataType = "VARBINARY";
            _column.Length = -1;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("BLOB", result);
        }

        [TestMethod]
        public void TestVarImage()
        {
            //arrange
            _column.DbDataType = "IMAGE";
            _column.Length = -1;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("BLOB", result);
        }

        [TestMethod]
        public void TestXml()
        {
            //arrange
            _column.DbDataType = "XML";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("XML", result);
        }

        [TestMethod]
        public void TestXmlType()
        {
            //arrange
            _column.DbDataType = "XMLTYPE";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("XML", result);
        }

        [TestMethod]
        public void TestGuid()
        {
            //arrange
            _column.DbDataType = "UNIQUEIDENTIFIER";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("CHAR(16) FOR BIT DATA", result);
        }
    }
}
