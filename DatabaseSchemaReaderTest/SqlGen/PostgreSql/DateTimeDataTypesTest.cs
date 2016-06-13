using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.PostgreSql
{
    [TestClass]
    public class DateTimeDataTypesTest
    {
        /*
date	 	calendar date (year, month, day)
interval [ fields ] [ (p) ]	 	time span
time [ (p) ] [ without time zone ]	 	time of day (no time zone)
time [ (p) ] with time zone	timetz	time of day, including time zone
timestamp [ (p) ] [ without time zone ]	 	date and time (no time zone)
timestamp [ (p) ] with time zone	timestamptz	date and time, including time zone
         */

        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();
        private readonly DatabaseColumn _column = new DatabaseColumn();

        [TestMethod]
        public void TestDateTime()
        {
            //arrange
            _column.DbDataType = "DATETIME";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIMESTAMP", result);
        }

        [TestMethod]
        public void TestDateTime2()
        {
            //arrange
            _column.DbDataType = "DATETIME2";

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

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("DATE", result);
        }

        [TestMethod]
        public void TestTime()
        {
            //arrange
            _column.DbDataType = "TIME";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("TIME", result);
        }

    }
}
