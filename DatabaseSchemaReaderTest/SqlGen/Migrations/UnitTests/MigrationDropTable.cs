using System.Linq;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.Migrations.UnitTests
{
    [TestClass]
    public class MigrationDropTable
    {
        [TestMethod]
        public void TestDropTable()
        {
            //arrange
            var migration = new DdlGeneratorFactory(SqlType.PostgreSql).MigrationGenerator();

            var schema = new DatabaseSchema(null, null);
            var customerTable = new DatabaseTable { Name = "Customers" };
            schema.AddTable(customerTable);
            customerTable.AddColumn<int>("Id")
                .AddPrimaryKey()
                .AddColumn<string>("Name").AddLength(30);
            var requestsTable = new DatabaseTable { Name = "Requests" };
            schema.AddTable(requestsTable);
            requestsTable.AddColumn<int>("Id")
                .AddPrimaryKey()
                .AddColumn<int>("CustomerId").AddForeignKey("FK_Order_Customer","Customers")
                .AddColumn<string>("Comment").AddLength(50);

            //act
            var addCustomer = migration.AddTable(customerTable);
            var addRequest = migration.AddTable(requestsTable);
            var addConstraint = migration.AddConstraint(requestsTable, requestsTable.ForeignKeys.First());
            var sql = migration.DropTable(customerTable);

            //assert
            Assert.IsTrue(sql.Contains("DROP TABLE IF EXISTS \"Customers\" CASCADE;"), "should include the drop table");

        }
    }
}
