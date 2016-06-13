using System;
using DatabaseSchemaReader.Data;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.InsertWriterTests
{
    [TestClass]
    public class DateConverterTest
    {
        [TestMethod]
        public void TestSqlServerDateTime()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.SqlServer;
            const string dbType = "DATETIME";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("'2001-03-30T10:45:30.839'", result);
        }

        [TestMethod]
        public void TestSqlServerDateTime2()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.SqlServer;
            const string dbType = "DATETIME2";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("'2001-03-30T10:45:30.8390000'", result);
        }

        [TestMethod]
        public void TestSqlServerDate()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.SqlServer;
            const string dbType = "DATE";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("'20010330'", result);
        }


        [TestMethod]
        public void TestSqlServerTime()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.SqlServer;
            const string dbType = "TIME";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("'10:45:30.839000'", result);
        }

        [TestMethod]
        public void TestOracleSimpleDate()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30);
            const SqlType sqlType = SqlType.Oracle;
            const string dbType = "DATE";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("DATE '2001-03-30'", result);
        }

        [TestMethod]
        public void TestOracleDate()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.Oracle;
            const string dbType = "DATE";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("TO_DATE('2001-03-30 10:45:30', 'yyyy-mm-dd hh24:mi:ss')", result);
        }

        [TestMethod]
        public void TestOracleTimestamp()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.Oracle;
            const string dbType = "TIMESTAMP";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("TIMESTAMP '2001-03-30 10:45:30.83'", result);
        }

        [TestMethod]
        public void TestOracleTimestamp6()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.Oracle;
            const string dbType = "TIMESTAMP(6)";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("TO_TIMESTAMP('2001-03-30 10:45:30.83900', 'yyyy-mm-dd hh24:mi:ss.FF')", result);
        }


        [TestMethod]
        public void TestDb2Date()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.Db2;
            const string dbType = "TIMESTAMP";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("'2001-03-30 10:45:30.839'", result);
        }


        [TestMethod]
        public void TestDb2Time()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.Db2;
            const string dbType = "TIME";

            var converter = new DateConverter(sqlType);

            //act
            var result = converter.Convert(dt, dbType);

            //assert
            Assert.AreEqual("'10:45:30.83'", result);
        }
    }
}
