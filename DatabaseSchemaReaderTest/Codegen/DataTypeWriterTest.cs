using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
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

namespace DatabaseSchemaReaderTest.Codegen
{


    /// <summary>
    ///Test the string representation of common datatypes
    ///</summary>
    [TestClass]
    public class DataTypeWriterTest
    {
        [TestMethod]
        public void TypeInteger()
        {
            var typewriter = new DataTypeWriter();

            var column = new DatabaseColumn();
            column.DataType = new DataType("NUMBER", "System.Int32");

            var result = typewriter.Write(column);

            Assert.AreEqual("int", result);
        }

        [TestMethod]
        public void TypeNullableInteger()
        {
            var typewriter = new DataTypeWriter();

            var column = new DatabaseColumn();
            column.DataType = new DataType("NUMBER", "System.Int32");
            column.Nullable = true;

            var result = typewriter.Write(column);

            Assert.AreEqual("int?", result);
        }

        [TestMethod]
        public void TypeString()
        {
            var typewriter = new DataTypeWriter();

            var column = new DatabaseColumn();
            column.DataType = new DataType("VARCHAR2", "System.String");

            var result = typewriter.Write(column);

            Assert.AreEqual("string", result);
        }

        [TestMethod]
        public void TypeNullableString()
        {
            var typewriter = new DataTypeWriter();

            var column = new DatabaseColumn();
            column.DataType = new DataType("VARCHAR2", "System.String");
            column.Nullable = true;

            var result = typewriter.Write(column);

            Assert.AreEqual("string", result);
        }
    }
}
