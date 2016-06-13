using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    [TestClass]
    public class DataTypesTest
    {
        /*
        --Character--
        CHAR(n) 
        NCHAR(n) 
        VARCHAR(n | max) 
        NVARCHAR(n | max) 
        TEXT 
        NTEXT 
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
        --Date and Time--
        DATE 
        TIME(p) 
        DATETIME 
        DATETIME2(p) 
        DATETIMEOFFSET(p) 
        SMALLDATETIME 
        --Binary--
        BINARY(n) 
        VARBINARY(n | max) 
        IMAGE 
        --Other--
        BIT 
        UNIQUEIDENTIFIER 
        XML 
        MONEY 
        SMALLMONEY 
        TIMESTAMP, ROWVERSION 
         */
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();
        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };

        [TestMethod]
        public void TestBlob()
        {
            //arrange
            _column.DbDataType = "BLOB";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("VARBINARY (MAX)", result);
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
            Assert.AreEqual("VARBINARY (MAX)", result);
        }


        [TestMethod]
        public void TestBinary()
        {
            //arrange
            _column.DbDataType = "BINARY";
            _column.Length = 400;
            //specify that we are coming from SqlServer, so keep Binary format
            var typeWriter = new DataTypeWriter(SqlType.SqlServer);

            //act
            var result = typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("BINARY (400)", result);
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
            Assert.AreEqual("IMAGE", result);
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
            Assert.AreEqual("UNIQUEIDENTIFIER", result);
        }
    }
}
