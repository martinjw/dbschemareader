using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.SqlServer
{
    [TestClass]
    public class DateTimeDataTypesTest
    {
        /*
        --Date and Time--
        DATE 
        TIME(p) 
        DATETIME 
        DATETIME2(p) 
        DATETIMEOFFSET(p) 
        SMALLDATETIME 
         */
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();
        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };

        [TestMethod]
        public void TestDateTime()
        {
            //arrange
            _column.DbDataType = "DATETIME";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATETIME", result);
        }

        [TestMethod]
        public void TestDateTime2()
        {
            //arrange
            _column.DbDataType = "DATETIME2";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATETIME2", result);
        }

        [TestMethod]
        public void TestTimeStamp()
        {
            //arrange
            _column.DbDataType = "TIMESTAMP";
            _column.Precision = 5;
            //this isn't a SqlServer Timestamp
            _column.DataType = null;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATETIME", result);
        }


        [TestMethod]
        public void TestSqlServerTimeStamp()
        {
            //arrange
            _column.DbDataType = "TIMESTAMP";
            _column.Precision = 18;
            _column.DataType = new DataType("TIMESTAMP", "byte[]");
            _column.DataType.ProviderDbType = (int)SqlDbType.Timestamp;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIMESTAMP", result);
        }

        [TestMethod]
        public void TestDate()
        {
            //arrange
            _column.DbDataType = "DATE";
            _column.DataType = new DataType("DATE", "System.DateTime");
            _column.DataType.ProviderDbType = (int)SqlDbType.Date;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATE", result);
        }

        [TestMethod]
        public void TestOracleDate()
        {
            //arrange
            _column.DbDataType = "DATE";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATETIME", result);
        }
    }
}
