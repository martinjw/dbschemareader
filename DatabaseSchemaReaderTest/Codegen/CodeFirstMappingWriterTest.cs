using System.Data;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{


    /// <summary>
    ///Test code first mapping
    ///</summary>
    [TestClass]
    public class CodeFirstMappingWriterTest
    {

        /// <summary>
        ///A test for Execute with CodeFirst
        ///</summary>
        [TestMethod]
        public void ExecuteTest()
        {
            //arrange
            DatabaseSchema schema = PrepareModel();
            var products = schema.FindTableByName("Products");
            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var target = new CodeFirstMappingWriter(products, codeWriterSettings, new MappingNamer());

            //act
            var result = target.Write();

            //assert
            var hasMappingClass = result.Contains("public class ProductMapping : EntityTypeConfiguration<Product>");
            //we don't have the UseForeignKeyIdProperties=true, so map back to the instance property
            var hasForeignKey =
                result.Contains("HasRequired(x => x.SupplierKey).WithMany(c => c.ProductCollection).Map(m => m.MapKey(\"SupplierKey\"))");
            var hasManyToMany = result.Contains("HasMany(x => x.CategoryCollection).WithMany(z => z.ProductCollection)");

            Assert.IsTrue(hasMappingClass);
            Assert.IsTrue(hasForeignKey);
            Assert.IsTrue(hasManyToMany);
        }

        [TestMethod]
        public void ForeignKeyIdTest()
        {
            //arrange
            DatabaseSchema schema = PrepareModel();
            var products = schema.FindTableByName("Products");
            var codeWriterSettings = new CodeWriterSettings
            {
                CodeTarget = CodeTarget.PocoEntityCodeFirst,
                UseForeignKeyIdProperties = true
            };
            var target = new CodeFirstMappingWriter(products, codeWriterSettings, new MappingNamer());

            //act
            var result = target.Write();

            //assert
            var hasMappingClass = result.Contains("public class ProductMapping : EntityTypeConfiguration<Product>");
            var hasForeignKeyIdProperty =
                result.Contains("Property(x => x.SupplierKeyId).HasColumnName(\"SupplierKey\")");
            var hasForeignKey = result.Contains("HasRequired(x => x.SupplierKey).WithMany(c => c.ProductCollection).HasForeignKey(c => c.SupplierKeyId);");

            Assert.IsTrue(hasMappingClass);
            Assert.IsTrue(hasForeignKeyIdProperty);
            Assert.IsTrue(hasForeignKey);
        }

        [TestMethod]
        public void HasMaxLengthTest()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            var table = schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey().AddIdentity()
                .AddColumn("ProductName", DbType.String).AddLength(20)
                .Table;
            //we need datatypes
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            schema.DataTypes.Add(new DataType("NVARCHAR", "System.String"));
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);

            var target = new CodeFirstMappingWriter(table, new CodeWriterSettings(), new MappingNamer());

            //act
            var result = target.Write();

            //assert
            var hasMaxLength = result.Contains("Property(x => x.ProductName).HasMaxLength(20)");

            Assert.IsTrue(hasMaxLength);
        }


        [TestMethod]
        public void IsMaxLengthTest()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            var table = schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey().AddIdentity()
                .AddColumn("ProductName", DbType.String).AddLength(-1)
                .Table;
            //we need datatypes
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            schema.DataTypes.Add(new DataType("NVARCHAR", "System.String"));
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);

            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var target = new CodeFirstMappingWriter(table, codeWriterSettings, new MappingNamer());

            //act
            var result = target.Write();

            //assert
            var hasMaxLength = result.Contains("Property(x => x.ProductName).IsMaxLength()");

            Assert.IsTrue(hasMaxLength);
        }


        [TestMethod]
        public void IsImageTest()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            var table = schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey().AddIdentity()
                .AddColumn("Picture", "image")
                .Table;
            //we need datatypes
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            schema.DataTypes.Add(new DataType("image", "System.Byte[]"));
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);

            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var target = new CodeFirstMappingWriter(table, codeWriterSettings, new MappingNamer());

            //act
            var result = target.Write();

            //assert
            //EF CF will default to varbinary so we must specify
            var hasImageType = result.Contains("Property(x => x.Picture).HasColumnType(\"image\")");

            Assert.IsTrue(hasImageType);
        }


        [TestMethod]
        public void MappingNameTest()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            var table = schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey().AddIdentity()
                .Table;
            table.NetName = "Product";
            //we need datatypes
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);

            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var target = new CodeFirstMappingWriter(table, codeWriterSettings, new MappingNamer());

            //act
            target.Write();
            //now the name is assigned
            var className = target.MappingClassName;

            //assert
            Assert.AreEqual("ProductMapping", className);
        }

        [TestMethod]
        public void MappingNameWithConflictsTest()
        {
            //arrange
            var schema = new DatabaseSchema(null, null);
            var table = schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey().AddIdentity()
                .Table;
            table.NetName = "Product";
            //we need datatypes
            schema.DataTypes.Add(new DataType("INT", "System.Int32"));
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);


            var mappingNamer = new MappingNamer();
            var target = new CodeFirstMappingWriter(table, new CodeWriterSettings(), mappingNamer);
            //we also have a table called "ProductMapping"
            //so we can't call the mapping class for "Product" the same name
            mappingNamer.EntityNames.Add("ProductMapping");
            mappingNamer.EntityNames.Add("ProductMappingMap");

            //act
            target.Write();
            //now the name is assigned
            var className = target.MappingClassName;

            //assert
            Assert.AreEqual("ProductMappingMapMap", className, "Should not conflict with the existing names");
            Assert.AreEqual(3, mappingNamer.EntityNames.Count, "Should add the name to the list");
        }

        [TestMethod]
        public void MappingSharedPrimaryKeyTest()
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

            var mappingNamer = new MappingNamer();
            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var target = new CodeFirstMappingWriter(table, codeWriterSettings, mappingNamer);

            //act
            var txt = target.Write();

            //assert
            var hasScalarKey = txt.Contains("HasKey(x => x.Regnum);");
            var hasForeignKey = txt.Contains("HasRequired(x => x.Vehicle);");

            Assert.IsTrue(hasScalarKey);
            Assert.IsTrue(hasForeignKey);
        }

        private static DatabaseSchema PrepareModel()
        {
            var schema = new DatabaseSchema(null, null);

            schema.AddTable("Categories")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddColumn("CategoryName", DbType.String);

            schema.AddTable("Suppliers")
                .AddColumn("SupplierId", DbType.Int32).AddPrimaryKey()
                .AddColumn("SupplierName", DbType.String);

            schema.AddTable("Products")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey().AddIdentity()
                .AddColumn("ProductName", DbType.String)
                .AddColumn("SupplierKey", DbType.Int32).AddForeignKey("fk", "Suppliers");

            schema.AddTable("CategoryProducts")
                .AddColumn("CategoryId", DbType.Int32).AddPrimaryKey()
                .AddForeignKey("fk", "Categories")
                .AddColumn("ProductId", DbType.Int32).AddPrimaryKey()
                .AddForeignKey("fk", "Products");

            DatabaseSchemaFixer.UpdateReferences(schema);
            PrepareSchemaNames.Prepare(schema, new Namer());

            return schema;
        }
    }
}
