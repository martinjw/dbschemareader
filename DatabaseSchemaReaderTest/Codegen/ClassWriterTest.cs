using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{


    /// <summary>
    /// Test class writer
    ///</summary>
    [TestClass]
    public class ClassWriterTest
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

            var cw = new ClassWriter(table, new CodeWriterSettings());

            //act
            var txt = cw.Write();

            //assert
            var hasName = txt.Contains("public class Category");
            var hasCategoryId = txt.Contains("public virtual int CategoryId { get; set; }");
            var hasCategoryName = txt.Contains("public virtual string CategoryName { get; set; }");

            Assert.IsTrue(hasName);
            Assert.IsTrue(hasCategoryId);
            Assert.IsTrue(hasCategoryName);
        }

        private static DatabaseSchema ArrangeSchema()
        {
            var schema = new DatabaseSchema(null, null);
            schema
                .AddTable("Categories")
                .AddColumn("CategoryId", "INT").AddPrimaryKey().AddIdentity()
                .AddColumn("CategoryName", "NVARCHAR")
                .AddTable("Products")
                .AddColumn("ProductId", "INT").AddPrimaryKey()
                .AddColumn("ProductName", "NVARCHAR")
                .AddColumn("CategoryId", "INT").AddForeignKey("fk", "Categories");
            //we need datatypes
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            schema.DataTypes.Add(new DataType("NVARCHAR", "System.String"));
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());
            return schema;
        }

        [TestMethod]
        public void WriteForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var productsTable = schema.FindTableByName("Products");

            var cw = new ClassWriter(productsTable, new CodeWriterSettings());

            //act
            var txt = cw.Write();

            //assert
            var hasCategory = txt.Contains("public virtual Category Category { get; set; }");

            Assert.IsTrue(hasCategory);
        }

        [TestMethod]
        public void WriteForeignKeyInverseTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var categoryTable = schema.FindTableByName("Categories");

            var cw = new ClassWriter(categoryTable, new CodeWriterSettings());

            //act
            var txt = cw.Write();

            //assert
            var hasProducts = txt.Contains("public virtual IList<Product> ProductCollection { get; private set; }");

            Assert.IsTrue(hasProducts);
        }

        [TestMethod]
        public void WriteForeignKeyInverseTestForNHibernate()
        {
            //arrange
            var schema = ArrangeSchema();
            var categoryTable = schema.FindTableByName("Categories");

            var codeWriterSettings = new CodeWriterSettings();
            codeWriterSettings.CodeTarget = CodeTarget.PocoNHibernateHbm;
            var cw = new ClassWriter(categoryTable, codeWriterSettings);

            //act
            var txt = cw.Write();

            //assert
            var hasProducts = txt.Contains("public virtual IList<Product> ProductCollection { get; protected set; }");

            Assert.IsTrue(hasProducts, "NHibernate 3.2 requires *all* setters to be protected or public");
        }

        [TestMethod]
        public void WriteForeignKeyInverseWithCollectionNamerTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var categoryTable = schema.FindTableByName("Categories");

            var cw = new ClassWriter(categoryTable, new CodeWriterSettings { Namer = new PluralizingNamer() });

            //act
            var txt = cw.Write();

            //assert
            var hasProducts = txt.Contains("public virtual IList<Product> Products { get; private set; }");

            Assert.IsTrue(hasProducts);
        }

        [TestMethod]
        public void WriteSharedPrimaryKeyTest()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            schema
                .AddTable("vehicle")
                .AddColumn<string>("regnum").AddPrimaryKey().AddLength(25)
                .AddColumn<string>("model").AddLength(32)
                .AddTable("car")
                .AddColumn<string>("regnum").AddLength(25).AddPrimaryKey().AddForeignKey("fk", "vehicle")
                .AddColumn<int>("doors");
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());
            var table = schema.FindTableByName("car");

            var cw = new ClassWriter(table, new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst });

            //act
            var txt = cw.Write();

            //assert
            var hasScalarKey = txt.Contains("public string Regnum { get; set; }");
            var hasForeignKey = txt.Contains("public virtual Vehicle Vehicle { get; set; }");

            Assert.IsTrue(hasScalarKey);
            Assert.IsTrue(hasForeignKey);
        }



        [TestMethod]
        public void WriteCodeFirstTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var categoryTable = schema.FindTableByName("Categories");

            var cw = new ClassWriter(categoryTable, new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst });

            //act
            var txt = cw.Write();

            //assert
            var hasCategoryId = txt.Contains("public int CategoryId { get; set; }");
            var hasProducts = txt.Contains("public virtual ICollection<Product> ProductCollection { get; private set; }");

            Assert.IsTrue(hasCategoryId, "Ordinary scalar properties don't need to be virtual");
            Assert.IsTrue(hasProducts);
        }

        [TestMethod]
        public void WriteViewTest()
        {
            //arrange
            var view = new DatabaseView();
            view.Name = "AlphabeticNames";
            view.AddColumn("FirstName", typeof(string)).AddNullable()
                .AddColumn("LastName", typeof(string)).AddNullable();

            var schema = new DatabaseSchema(null, null);
            schema.Views.Add(view);
            PrepareSchemaNames.Prepare(schema, new Namer());

            var codeWriterSettings = new CodeWriterSettings
            {
                CodeTarget = CodeTarget.PocoNHibernateHbm,
                IncludeViews = true
            };
            var cw = new ClassWriter(view, codeWriterSettings);

            //act
            var txt = cw.Write();

            //assert
            var hasFirstName = txt.Contains("public virtual string FirstName");
            var hasLastName = txt.Contains("public virtual string LastName");
            var hasEquals = txt.Contains("public override bool Equals(object obj)");

            Assert.IsTrue(hasFirstName);
            Assert.IsTrue(hasLastName);
            Assert.IsTrue(hasEquals);
        }

    }
}
