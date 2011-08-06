using System.Data;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.MySql;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
#endif

namespace DatabaseSchemaReaderTest.SqlGen.MySql
{
    [TestClass]
    public class DateTimeDataTypesTest
    {
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
            Assert.AreEqual("DATETIME", result);
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
            Assert.AreEqual("TIMESTAMP", result); //timestamp has no precision
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
            Assert.AreEqual("TINYBLOB", result);
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
    }
}
