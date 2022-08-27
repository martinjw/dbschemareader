using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationDropTable
    {
        [TestMethod]
        public void TestSqlServer()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.SqlServer).MigrationGenerator();

            var schema = new DatabaseSchema(null, null);
            var customerTable = new DatabaseTable { Name = "Customers" };
            schema.AddTable(customerTable);
            customerTable.DatabaseSchema = schema;
            schema.Tables.Add(customerTable);
            customerTable.AddColumn<int>("Id")
                .AddPrimaryKey()
                .AddColumn<string>("Name").AddLength(30);
            var orderTable = new DatabaseTable { Name = "Requests" };
            schema.AddTable(orderTable);
            orderTable.AddColumn<int>("Id")
                .AddPrimaryKey()
                .AddColumn<int>("CustomerId").AddForeignKey("FK_Order_Customer","Customers")
                .AddColumn<string>("Comment").AddLength(50);

            //act
            var sql = migration.DropTable(customerTable);

            //assert
            Assert.IsTrue(sql.Contains("DROP TABLE [Customers];"), "should include the drop table");

        }
    }
}
