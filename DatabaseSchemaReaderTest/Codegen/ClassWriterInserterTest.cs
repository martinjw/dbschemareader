using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        class CustomCodeInserter : CodeInserter
        {
            public override void WriteNamespaces(DatabaseTable table, ClassBuilder classBuilder)
            {
                classBuilder.AppendLine("using System.ComponentModel.DataAnnotations.Schema");
            }

            public override void WriteTableAnnotations(DatabaseTable table, ClassBuilder classBuilder)
            {
                classBuilder.AppendFormat("[Table(\"{0}\")]", table.Name);
            }

            public override void WriteColumnAnnotations(DatabaseTable table, DatabaseColumn column, ClassBuilder classBuilder)
            {
                classBuilder.AppendFormat("[Column(\"{0}\")]", column.Name);
            }
        }
    }
}
