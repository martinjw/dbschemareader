using System.Collections.Generic;
using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.Databases.Oracle;
using DatabaseSchemaReader.SqlGen.Oracle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Oracle
{
    [TestClass]
    public class DateTimeDataTypesTest
    {
        /*
          --Dates--
        DATE
        TIMESTAMP (fractional_seconds_precision) 
        TIMESTAMP (fractional_seconds_precision) WITH {LOCAL} TIMEZONE
        INTERVAL YEAR (year_precision) TO MONTH
        INTERVAL DAY (day_precision) TO SECOND (fractional_seconds_precision) 
         */

        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };
        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();

        [TestMethod]
        public void TestDateTime()
        {
            //arrange
            _column.DbDataType = "DATETIME";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATE", result);
        }

        [TestMethod]
        public void TestDateTime2()
        {
            //arrange
            _column.DbDataType = "DATETIME2";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIMESTAMP (6)", result);
        }

        [TestMethod]
        public void TestTimeStamp()
        {
            //arrange
            _column.DbDataType = "TIMESTAMP";
            _column.Precision = 5;
            _column.DataType = null;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIMESTAMP (5)", result);
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
            Assert.AreEqual("NUMBER (18)", result);
        }

        [TestMethod]
        public void TestSqlServerTimeStampWithTimeZone()
        {
            //arrange
            _column.DbDataType = "TIMESTAMP(6) WITH TIME ZONE";
            _column.Precision = 6;
            //automatically assign oracle datatypes
            var types = new DataTypeList().Execute();
            DatabaseSchemaFixer.UpdateDataTypes(types, new List<DatabaseColumn>(new []{_column}));

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIMESTAMP (6) WITH TIME ZONE", result);
        }

        [TestMethod]
        public void TestDate()
        {
            //arrange
            _column.DbDataType = "DATE";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATE", result);
        }

        [TestMethod]
        public void TestDateWithDefaultCurrentDate()
        {
            //arrange
            _column.DbDataType = "DATE";
            _column.DefaultValue = "current_date";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATE DEFAULT CURRENT_DATE", result);
        }

        [TestMethod]
        public void TestDateWithDefaultSysDate()
        {
            //arrange
            _column.DbDataType = "DATE";
            _column.DefaultValue = "SYSDATE";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATE DEFAULT SYSDATE", result);
        }

        [TestMethod]
        public void TestDateWithDefaultDate()
        {
            //arrange
            _column.DbDataType = "DATE";
            _column.DefaultValue = "1999-12-31"; //assume the string is ANSI standard, otherwise they'd use TO_DATE

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATE DEFAULT DATE '1999-12-31'", result);
        }

        [TestMethod]
        public void TestTimestamp()
        {
            //arrange
            _column.DbDataType = "TIMESTAMP";
            _column.DefaultValue = "systimestamp";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIMESTAMP (6) DEFAULT systimestamp", result);
        }
    }
}
