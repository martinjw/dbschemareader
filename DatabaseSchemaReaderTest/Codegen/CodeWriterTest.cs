using System.Data;
using System.IO;
using System.Linq;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    /// <summary>
    ///Create a simple model and write it to filesystem
    ///</summary>
    [TestClass]
    public class CodeWriterTest
    {
        /// <summary>
        ///A test for Pocos
        ///</summary>
        [TestMethod]
        public void PocoTest()
        {
            DatabaseSchema schema = PrepareModel();

            var directory = TestHelper.CreateDirectory("MyTest");
            const string @namespace = "MyTest";
            var settings = new CodeWriterSettings { Namespace = @namespace, CodeTarget = CodeTarget.Poco };

            var target = new CodeWriter(schema, settings);
            target.Execute(directory);

            var files = directory.GetFiles("*.cs");
            var products = files.FirstOrDefault(f => f.Name == "Product.cs");
            Assert.IsNotNull(products, "Should have written Product class to represent [Products] table");

            var category = files.FirstOrDefault(f => f.Name == "Category.cs");
            Assert.IsNotNull(category, "Should have written Category class to represent [Categories] table");

            var cs = File.ReadAllText(category.FullName);

            var ok = cs.Contains("public virtual IList<Product> ProductCollection { get; private set; }");
            Assert.IsTrue(ok, "Should contain the collection of products");
        }

        /// <summary>
        ///A test for NHibernateFluent
        ///</summary>
        [TestMethod]
        public void NHibernateFluentTest()
        {
            var schema = PrepareModel();

            var directory = TestHelper.CreateDirectory("MyTest");
            const string @namespace = "MyTest";
            var settings = new CodeWriterSettings { Namespace = @namespace, CodeTarget = CodeTarget.PocoNHibernateFluent };

            var target = new CodeWriter(schema, settings);
            target.Execute(directory);

            var mapping = directory.GetDirectories("mapping").FirstOrDefault();
            if (mapping == null)
                Assert.Fail("Could not find Mapping subdirectory");
            var files = mapping.GetFiles("*.cs");
            var products = files.FirstOrDefault(f => f.Name == "ProductMapping.cs");
            Assert.IsNotNull(products, "Should have written Product class to map [Products] table");

            var category = files.FirstOrDefault(f => f.Name == "CategoryMapping.cs");
            Assert.IsNotNull(category, "Should have written Category class to map [Categories] table");

            var cs = File.ReadAllText(category.FullName);

            var ok = cs.Contains("public class CategoryMapping : ClassMap<Category>");
            Assert.IsTrue(ok, "Should contain the ClassMap<>");
        }

        /// <summary>
        ///A test for Views
        ///</summary>
        [TestMethod]
        public void ViewTest()
        {
            var schema = PrepareModel();

            var directory = TestHelper.CreateDirectory("MyViewTest");
            const string @namespace = "MyTest";
            var settings = new CodeWriterSettings { Namespace = @namespace, CodeTarget = CodeTarget.Poco, IncludeViews = true };

            var target = new CodeWriter(schema, settings);
            target.Execute(directory);

            var files = directory.GetFiles("*.cs");
            var category = files.FirstOrDefault(f => f.Name == "ActiveCategory.cs");
            Assert.IsNotNull(category, "Should have written ActiveCategory class to map [ActiveCategories] view");
        }

        /// <summary>
        ///A test for Stored Procedures
        ///</summary>
        [TestMethod]
        public void ProcedureTest()
        {
            var schema = PrepareModel();

            var procedure = new DatabaseStoredProcedure { Name = "SelectCategory" };
            var argument = new DatabaseArgument
                           {
                               Name = "p1",
                               DatabaseDataType = "VARCHAR",
                               Length = 10,
                               DataType = DataTypeConverter.FindDataType("VARCHAR", schema.DataTypes, SqlType.SqlServer, null),
                               In = true,
                           };
            procedure.Arguments.Add(argument);
            var rs = new DatabaseResultSet();
            var resultColumn = new DatabaseColumn { Name = "Output", DbDataType = "VARCHAR" };
            DataTypeConverter.AddDataType(resultColumn);
            rs.Columns.Add(resultColumn);
            procedure.ResultSets.Add(rs);
            schema.StoredProcedures.Add(procedure);

            var directory = TestHelper.CreateDirectory("MySprocTest");
            const string @namespace = "MySprocTest";
            var settings = new CodeWriterSettings
            {
                Namespace = @namespace,
                CodeTarget = CodeTarget.Poco,
                WriteStoredProcedures = true
            };

            var target = new CodeWriter(schema, settings);
            target.Execute(directory);

            var procedures = directory.GetDirectories("Procedures").FirstOrDefault();
            if (procedures == null)
                Assert.Fail("Could not find Procedures subdirectory");
            var files = procedures.GetFiles("*.cs");
            var products = files.FirstOrDefault(f => f.Name == "SelectCategory.cs");
            Assert.IsNotNull(products, "Should have written SelectCategory class for SelectCategory procedure");

            var category = files.FirstOrDefault(f => f.Name == "SelectCategoryResult.cs");
            Assert.IsNotNull(category, "Should have written SelectCategoryResult class to the result of the sproc");
        }

        private static DatabaseSchema PrepareModel()
        {
            var schema = new DatabaseSchema(null, null);

            schema.AddTable("Categories")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddColumn("CategoryName", DbType.String);

            schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey()
                .AddColumn("ProductName", DbType.String)
                .AddColumn("CategoryId", DbType.Int32).AddForeignKey("fk", "Categories");

            var view = new DatabaseView { Name = "ActiveCategories" };
            view.AddColumn<string>("CategoryName");
            schema.Views.Add(view);

            DatabaseSchemaFixer.UpdateReferences(schema);

            return schema;
        }
    }
}
