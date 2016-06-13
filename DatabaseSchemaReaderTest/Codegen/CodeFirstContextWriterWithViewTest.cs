using System.Collections.Generic;
using System.Data;
using System.Linq;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{

    /// <summary>
    /// Test code first DbContext
    ///</summary>
    [TestClass]
    public class CodeFirstContextWriterWithViewTest
    {

        /// <summary>
        ///A test for Execute with CodeFirst
        ///</summary>
        [TestMethod]
        public void ExecuteTest()
        {
            //arrange
            DatabaseSchema schema = PrepareModel();
            var settings = new CodeWriterSettings();
            settings.IncludeViews = true;
            var target = new CodeFirstContextWriter(settings);
            var list = new List<DatabaseTable>(schema.Tables);
            list.AddRange(schema.Views.OfType<DatabaseTable>());

            //act
            var result = target.Write(list);

            //assert
            var hasDbSet = result.Contains("public IDbSet<AlphabeticCategory> AlphabeticCategoryCollection");
            var hasMapping = result.Contains("modelBuilder.Configurations.Add(new AlphabeticCategoryMapping());");
            Assert.IsTrue(hasDbSet);
            Assert.IsTrue(hasMapping);
        }

        private static DatabaseSchema PrepareModel()
        {
            var schema = new DatabaseSchema(null, null);

            schema.AddTable("Categories")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddColumn("CategoryName", DbType.String);

            var view = new DatabaseView { Name = "AlphabeticCategories"};
            schema.Views.Add(view);
            view
                .AddColumn("CategoryId", DbType.Int32)
                .AddColumn("CategoryName", DbType.String);

            DatabaseSchemaFixer.UpdateReferences(schema);
            PrepareSchemaNames.Prepare(schema, new Namer());

            return schema;
        }
    }
}
