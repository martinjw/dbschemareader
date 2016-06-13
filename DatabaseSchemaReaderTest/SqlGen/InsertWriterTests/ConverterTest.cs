using System;
using System.Collections.Generic;
using DatabaseSchemaReader.Data;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.InsertWriterTests
{
    /// <summary>
    /// Tests converting data to INSERT strings
    /// </summary>
    [TestClass]
    public class ConverterTest
    {

        [TestMethod]
        public void TestNull()
        {
            //arrange
            string s = null;
            const SqlType sqlType = SqlType.Db2;
            var dateTypes = new Dictionary<string, string>();

            var converter = new Converter(sqlType, dateTypes);

            //act
            var result = converter.Convert(typeof(string), s, "Name");

            //assert
            Assert.AreEqual("NULL", result);
        }

        [TestMethod]
        public void TestDbNull()
        {
            //arrange
            var dbNull = DBNull.Value;
            const SqlType sqlType = SqlType.Db2;
            var dateTypes = new Dictionary<string, string>();

            var converter = new Converter(sqlType, dateTypes);

            //act
            var result = converter.Convert(typeof(string), dbNull, "Name");

            //assert
            Assert.AreEqual("NULL", result);
        }

        [TestMethod]
        public void TestString()
        {
            //arrange
            const string s = "Hello";
            const SqlType sqlType = SqlType.Db2;
            var dateTypes = new Dictionary<string, string>();

            var converter = new Converter(sqlType, dateTypes);

            //act
            var result = converter.Convert(typeof(string), s, "Name");

            //assert
            Assert.AreEqual("'Hello'", result);
        }

        [TestMethod]
        public void TestStringWithSingleQuotes()
        {
            //arrange
            const string s = "Hello 'Boys'";
            const SqlType sqlType = SqlType.Db2;
            var dateTypes = new Dictionary<string, string>();

            var converter = new Converter(sqlType, dateTypes);

            //act
            var result = converter.Convert(typeof(string), s, "Name");

            //assert
            Assert.AreEqual("'Hello ''Boys'''", result);
        }

        [TestMethod]
        public void TestStringUnicodeSqlServer()
        {
            //arrange
            const string s = "Hello";
            const SqlType sqlType = SqlType.SqlServer;
            var dateTypes = new Dictionary<string, string>();

            var converter = new Converter(sqlType, dateTypes);

            //act
            var result = converter.Convert(typeof(string), s, "Name");

            //assert
            Assert.AreEqual("N'Hello'", result);
        }


        [TestMethod]
        public void TestStringInteger()
        {
            //arrange
            const int i = 10;
            const SqlType sqlType = SqlType.SqlServer;
            var dateTypes = new Dictionary<string, string>();

            var converter = new Converter(sqlType, dateTypes);

            //act
            var result = converter.Convert(typeof(int), i, "Name");

            //assert
            Assert.AreEqual("10", result);
        }

        [TestMethod]
        public void TestStringDecimal()
        {
            //arrange
            const decimal i = 10.5M;
            const SqlType sqlType = SqlType.SqlServer;
            var dateTypes = new Dictionary<string, string>();

            var converter = new Converter(sqlType, dateTypes);

            //act
            var result = converter.Convert(typeof(decimal), i, "Name");

            //assert
            Assert.AreEqual("10.5", result);
        }

        [TestMethod]
        public void TestDate()
        {
            //arrange
            var dt = new DateTime(2001, 3, 30, 10, 45, 30, 839);
            const SqlType sqlType = SqlType.Db2;
            var dateTypes = new Dictionary<string, string>();
            dateTypes.Add("StartDate", "TIMESTAMP");

            var converter = new Converter(sqlType, dateTypes);

            //act
            var result = converter.Convert(typeof(DateTime), dt, "StartDate");

            //assert
            Assert.AreEqual("'2001-03-30 10:45:30.839'", result);
        }


        [TestMethod]
        public void TestStringTimeSpan()
        {
            //arrange
            var ts = new TimeSpan(1, 2, 3);
            const SqlType sqlType = SqlType.SqlServer;
            var dateTypes = new Dictionary<string, string>();

            var converter = new Converter(sqlType, dateTypes);

            //act
            var result = converter.Convert(typeof(TimeSpan), ts, "Name");

            //assert
            Assert.AreEqual("'01:02:03'", result);
        }
    }
}
