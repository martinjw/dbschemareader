using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class TableExistsTests
    {
        [TestMethod, TestCategory("SqlServer")]
        public void TableExists()
        {
            //arrange
            var dbReader = TestHelper.GetNorthwindReader();

            //act
            var result = dbReader.TableExists("Products");
            //assert
            Assert.IsTrue(result, "Products table should exist in Northwind database");
        }

        [TestMethod, TestCategory("SqlServer")]
        public void TableDoesNotExist()
        {
            //arrange
            var dbReader = TestHelper.GetNorthwindReader();
            //act
            var result = dbReader.TableExists("Does_Not_Exist");
            //assert
            Assert.IsFalse(result, "Does_Not_Exist table should not exist in Northwind database");
        }
    }
}
