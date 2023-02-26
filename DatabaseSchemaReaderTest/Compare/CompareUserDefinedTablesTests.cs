using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Compare
{
    [TestClass]
    public class CompareUserDefinedTablesTests
    {
        private static UserDefinedTable BuidUdt(int length)
        {
            var udt = new UserDefinedTable();
            udt.Name = "Name";
            udt.AddColumn<int>("Id").AddNullable();
            udt.AddColumn<string>("Name").AddLength(length);
            return udt;
        }

        [TestMethod]
        public void TestUdtCompare()
        {
            var schema1 = new DatabaseSchema(null, SqlType.SqlServer);
            schema1.UserDefinedTables.Add(BuidUdt(20));

            var schema2 = new DatabaseSchema(null, SqlType.SqlServer);
            schema2.UserDefinedTables.Add(BuidUdt(30));

            var comparison = new CompareSchemas(schema1, schema2);
            var script = comparison.Execute();

            //comment for now
            Assert.IsTrue(script.Contains("Name"));
        }
    }
}