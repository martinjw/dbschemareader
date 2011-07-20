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
    public class DataTypesTest
    {
        private readonly DatabaseColumn _column = new DatabaseColumn { Nullable = true };

        [TestMethod]
        public void TestBlob()
        {
            //arrange
            _column.DbDataType = "BLOB";

            //act
            var result = DataTypeWriter.MySqlDataType(_column);

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
            var result = DataTypeWriter.MySqlDataType(_column);

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
            var result = DataTypeWriter.MySqlDataType(_column);

            //assert
            Assert.AreEqual("BLOB", result);
        }

        [TestMethod]
        public void TestXml()
        {
            //arrange
            _column.DbDataType = "XML";

            //act
            var result = DataTypeWriter.MySqlDataType(_column);

            //assert
            Assert.AreEqual("TEXT", result);
        }

        [TestMethod]
        public void TestXmlType()
        {
            //arrange
            _column.DbDataType = "XMLTYPE";

            //act
            var result = DataTypeWriter.MySqlDataType(_column);

            //assert
            Assert.AreEqual("TEXT", result);
        }

        [TestMethod]
        public void TestGuid()
        {
            //arrange
            _column.DbDataType = "UNIQUEIDENTIFIER";

            //act
            var result = DataTypeWriter.MySqlDataType(_column);

            //assert
            Assert.AreEqual("VARCHAR (64)", result);
        }
    }
}
