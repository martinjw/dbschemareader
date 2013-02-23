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
    /// Test class writer with foreign keys
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
                .AddColumn<int>("BillingAddress").AddForeignKey("fk", "Address")
                .AddColumn<int>("DeliveryAddress").AddForeignKey("fk", "Address")
                ;
            //make sure it's all tied up
            DatabaseSchemaFixer.UpdateReferences(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());
            return schema;
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
            //first one has the default name
            var hasBillingAddress = txt.Contains("public virtual IList<Order> BillingAddressCollection { get; private set; }");
            var hasDeliveryAddress = txt.Contains("public virtual IList<Order> DeliveryAddressCollection { get; private set; }");

            Assert.IsTrue(hasBillingAddress);
            Assert.IsTrue(hasDeliveryAddress);
        }



   }
}
