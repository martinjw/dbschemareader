using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen
{
    [TestClass]
    public class DataTypeParserTest
    {
        [TestMethod]
        public void TestInt()
        {
            var column = DataTypeConverter.ParseDataType("INT");

            Assert.AreEqual("INT", column.DbDataType);
            Assert.IsNull(column.Length);
            Assert.IsNull(column.Precision);
        }

        [TestMethod]
        public void TestVarChar()
        {
            var column = DataTypeConverter.ParseDataType("VARCHAR(10)");

            Assert.AreEqual("VARCHAR", column.DbDataType);
            Assert.AreEqual(10, column.Length);
            Assert.IsNull(column.Precision);
        }


        [TestMethod]
        public void TestVarCharMax()
        {
            var column = DataTypeConverter.ParseDataType("VARCHAR(MAX)");

            Assert.AreEqual("VARCHAR", column.DbDataType);
            Assert.AreEqual(-1, column.Length);
            Assert.IsNull(column.Precision);
        }

        [TestMethod]
        public void TestDecimal()
        {
            var column = DataTypeConverter.ParseDataType("DECIMAL ( 10 )");

            Assert.AreEqual("DECIMAL", column.DbDataType);
            Assert.AreEqual(null, column.Length);
            Assert.AreEqual(10, column.Precision);
        }


        [TestMethod]
        public void TestDecimalWithScale()
        {
            var column = DataTypeConverter.ParseDataType("DECIMAL ( 10 , 2 )");

            Assert.AreEqual("DECIMAL", column.DbDataType);
            Assert.AreEqual(null, column.Length);
            Assert.AreEqual(10, column.Precision);
            Assert.AreEqual(2, column.Scale);
        }
    }
}
