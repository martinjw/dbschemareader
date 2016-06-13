using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{
    [TestClass]
    public class ClassWriterTablePerTypeTest
    {
        private static DatabaseSchema Arrange()
        {
            var schema = new DatabaseSchema(null, null);
            schema.AddTable("Vehicles")
                  .AddColumn<int>("Vehicle_Id").AddPrimaryKey()
                  .AddColumn<string>("ModelName")

                  .AddTable("Cars")
                  .AddColumn<int>("Vehicle_Id").AddPrimaryKey().AddForeignKey("Vehicles")
                  .AddColumn<int>("Seats")

                  .AddTable("Bicycles")
                  .AddColumn<int>("Vehicle_Id").AddPrimaryKey().AddForeignKey("Vehicles")
                  .AddColumn<string>("Gears");

            DatabaseSchemaFixer.UpdateDataTypes(schema);
            //make sure .Net names are assigned
            PrepareSchemaNames.Prepare(schema, new Namer());

            return schema;
        }


        [TestMethod]
        public void TestTablePerTypeSubClass()
        {
            //arrange
            var schema = Arrange();
            //var schema = Arrange();
            var table = schema.FindTableByName("Cars");
            var codeWriterSettings = new CodeWriterSettings { CodeTarget = CodeTarget.PocoEntityCodeFirst };

            var classWriter = new ClassWriter(table, codeWriterSettings);

            //act
            var txt = classWriter.Write();

            //assert
            Assert.IsTrue(txt.Contains("public class Car : Vehicle"), "Subclass inherits from parent class");
            Assert.IsFalse(txt.Contains("public int VehicleId { get; set; }"), "The primary key is marked on the parent class");
        }


    }
}
