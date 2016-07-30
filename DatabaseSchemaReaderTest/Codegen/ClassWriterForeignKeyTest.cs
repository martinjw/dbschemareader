using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.CodeGen.NHibernate;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{


    /// <summary>
    /// Test class writer and all mappings with multiple foreign keys
    ///</summary>
    [TestClass]
    public class ClassWriterForeignKeyTest
    {
        private static DatabaseSchema ArrangeSchema()
        {
            var schema = new DatabaseSchema(null, null);
            schema
                .AddTable("Address")
                .AddColumn<int>("Address_Id").AddPrimaryKey().AddIdentity()
                .AddColumn<string>("Street")
                .AddColumn<string>("City")
                .AddTable("Orders")
                .AddColumn<int>("Order_Id").AddPrimaryKey().AddIdentity()
                .AddColumn<string>("Name")
                .AddColumn<int>("BillingAddress").AddForeignKey("fk_Orders_Address_Billing", "Address")
                .AddColumn<int>("DeliveryAddress").AddForeignKey("fk_Orders_Address_Delivery", "Address")
                ;
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());
            return schema;
        }

        [TestMethod]
        public void WriteMultipleForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var table = schema.FindTableByName("Orders");

            var cw = new ClassWriter(table, new CodeWriterSettings());

            //act
            var txt = cw.Write();

            //assert
            var hasBillingAddress = txt.Contains("public virtual Address BillingAddress { get; set; }");
            var hasDeliveryAddress = txt.Contains("public virtual Address DeliveryAddress { get; set; }");

            Assert.IsTrue(hasBillingAddress);
            Assert.IsTrue(hasDeliveryAddress);
        }

        [TestMethod]
        public void WriteInverseForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var table = schema.FindTableByName("Address");

            var cw = new ClassWriter(table, new CodeWriterSettings());

            //act
            var txt = cw.Write();

            //assert
            var hasBillingAddress = txt.Contains("public virtual IList<Order> BillingAddressCollection { get; private set; }");
            var hasDeliveryAddress = txt.Contains("public virtual IList<Order> DeliveryAddressCollection { get; private set; }");

            Assert.IsTrue(hasBillingAddress);
            Assert.IsTrue(hasDeliveryAddress);
        }

        [TestMethod]
        public void WriteCodeFirstMappingForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var table = schema.FindTableByName("Address");

            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var cw = new CodeFirstMappingWriter(table, codeWriterSettings, new MappingNamer());

            //act
            var txt = cw.Write();

            //assert
            var hasBillingAddress = txt.Contains("HasMany(x => x.BillingAddressCollection);");
            var hasDeliveryAddress = txt.Contains("HasMany(x => x.DeliveryAddressCollection);");

            Assert.IsTrue(hasBillingAddress);
            Assert.IsTrue(hasDeliveryAddress);
        }

        [TestMethod]
        public void WriteCodeFirstMappingInverseForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var table = schema.FindTableByName("Orders");

            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var cw = new CodeFirstMappingWriter(table, codeWriterSettings, new MappingNamer());

            //act
            var txt = cw.Write();

            //assert
            var hasBillingAddress = txt.Contains("HasRequired(x => x.BillingAddress).WithMany(c => c.BillingAddressCollection).Map(m => m.MapKey(\"BillingAddress\"));");
            var hasDeliveryAddress = txt.Contains("HasRequired(x => x.DeliveryAddress).WithMany(c => c.DeliveryAddressCollection).Map(m => m.MapKey(\"DeliveryAddress\"));");

            Assert.IsTrue(hasBillingAddress);
            Assert.IsTrue(hasDeliveryAddress);
        }


        [TestMethod]
        public void WriteNHibernateMappingForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var table = schema.FindTableByName("Address");

            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var cw = new MappingWriter(table, codeWriterSettings);

            //act
            var txt = cw.Write();

            //assert
            var hasBillingAddress = txt.Contains("<bag name=\"BillingAddressCollection\" table=\"`Orders`\"");
            var hasDeliveryAddress = txt.Contains("<bag name=\"DeliveryAddressCollection\" table=\"`Orders`\"");

            Assert.IsTrue(hasBillingAddress);
            Assert.IsTrue(hasDeliveryAddress);
        }

        [TestMethod]
        public void WriteNHibernateMappingInverseForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var table = schema.FindTableByName("Orders");

            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };
            var cw = new MappingWriter(table, codeWriterSettings);

            //act
            var txt = cw.Write();

            //assert
            var hasBillingAddress = txt.Contains("<many-to-one name=\"BillingAddress\" class=\"Address\" />");
            var hasDeliveryAddress = txt.Contains("<many-to-one name=\"DeliveryAddress\" class=\"Address\" />");

            Assert.IsTrue(hasBillingAddress);
            Assert.IsTrue(hasDeliveryAddress);
        }


        [TestMethod]
        public void WriteFluentNHibernateMappingForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var table = schema.FindTableByName("Address");

            var cw = new FluentMappingWriter(table, new CodeWriterSettings(), new MappingNamer());

            //act
            var txt = cw.Write();

            //assert
            var hasBillingAddress = txt.Contains("HasMany(x => x.BillingAddressCollection).KeyColumn(\"BillingAddress\").Inverse()");
            var hasDeliveryAddress = txt.Contains("HasMany(x => x.DeliveryAddressCollection).KeyColumn(\"DeliveryAddress\").Inverse()");

            Assert.IsTrue(hasBillingAddress);
            Assert.IsTrue(hasDeliveryAddress);
        }

        [TestMethod]
        public void WriteFluentNHibernateMappingInverseForeignKeyTest()
        {
            //arrange
            var schema = ArrangeSchema();
            var table = schema.FindTableByName("Orders");

            var cw = new FluentMappingWriter(table, new CodeWriterSettings(), new MappingNamer());

            //act
            var txt = cw.Write();

            //assert
            var hasBillingAddress = txt.Contains("References(x => x.BillingAddress).Column(\"BillingAddress\");");
            var hasDeliveryAddress = txt.Contains("References(x => x.DeliveryAddress).Column(\"DeliveryAddress\");");

            Assert.IsTrue(hasBillingAddress);
            Assert.IsTrue(hasDeliveryAddress);
        }

    }
}
