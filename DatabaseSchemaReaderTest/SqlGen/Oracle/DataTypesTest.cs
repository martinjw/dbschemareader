using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Oracle
{
    [TestClass]
    public class DataTypesTest
    {
        /*
        --Character--
        VARCHAR2(size)
        NVARCHAR2(size)
        VARCHAR **Obsolete**
        CHAR(size)
        NCHAR(size)
        --Numbers--
        NUMBER(p,s)
        PLS_INTEGER
        BINARY_INTEGER
        LONG
        --Dates--
        DATE
        TIMESTAMP (fractional_seconds_precision) 
        TIMESTAMP (fractional_seconds_precision) WITH {LOCAL} TIMEZONE
        INTERVAL YEAR (year_precision) TO MONTH
        INTERVAL DAY (day_precision) TO SECOND (fractional_seconds_precision) 
        --Large--
        CLOB
        NCLOB
        BLOB
        --Other--
        RAW(size)
        LONG RAW
        ROWID
        UROWID
        MLSLABEL
        BFILE
        XMLType
         */

        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();

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
            Assert.AreEqual("XMLTYPE", result);
        }

        [TestMethod]
        public void TestXmlType()
        {
            //arrange
            _column.DbDataType = "XMLTYPE";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("XMLTYPE", result);
        }

        [TestMethod]
        public void TestGuid()
        {
            //arrange
            _column.DbDataType = "UNIQUEIDENTIFIER";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("RAW(16)", result);
        }
    }
}
