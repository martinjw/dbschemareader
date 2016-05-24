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
    /// Test class writer
    ///</summary>
    [TestClass]
    public class ClassWriterInserterTest
    {

        /// <summary>
        ///A test for write
        ///</summary>
        [TestMethod]
        public void WriteTest()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            var table = schema.AddTable("Categories")
                .AddColumn("CategoryId", "INT").AddPrimaryKey().AddIdentity()
                .AddColumn("CategoryName", "NVARCHAR").Table;
            //we need datatypes
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            schema.DataTypes.Add(new DataType("NVARCHAR", "System.String"));
            DatabaseSchemaFixer.UpdateDataTypes(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());

            //inject the custom code inserter
            var codeWriterSettings = new CodeWriterSettings {CodeInserter = new CustomCodeInserter()};
            var cw = new ClassWriter(table, codeWriterSettings);

            //act
            var txt = cw.Write();

            //assert
            Assert.IsTrue(txt.Contains("using System.ComponentModel.DataAnnotations.Schema"));
            Assert.IsTrue(txt.Contains("[Table(\"Categories\")]"));
            Assert.IsTrue(txt.Contains("[Column(\"CategoryId\")]"));
        }

        public class CustomCodeInserter : CodeInserter
        {
            public override string WriteNamespaces(DatabaseTable table)
            {
                return "using System.ComponentModel.DataAnnotations.Schema";
            }

            public override string WriteTableAnnotations(DatabaseTable table)
            {
                return "[Table(\"" + table.Name + "\")]";
            }

            public override string WriteColumnAnnotations(DatabaseTable table, DatabaseColumn column)
            {
                return "[Column(\"" + column.Name + "\")]";
            }
        }
    }
}
