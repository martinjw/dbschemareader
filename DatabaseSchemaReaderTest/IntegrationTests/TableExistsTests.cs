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
            var result = false;
            //act
            if (!TestHelper.GetNorthwindReader(reader => result = reader.TableExists("Products")))
            {
                return;
            }

            //assert
            Assert.IsTrue(result, "Products table should exist in Northwind database");
        }

        [TestMethod, TestCategory("SqlServer")]
        public void TableDoesNotExist()
        {
            //arrange
            var result = true;
            //act
            if (!TestHelper.GetNorthwindReader(reader => result = reader.TableExists("Does_Not_Exist")))
            {
                return;
            }            
            //assert
            Assert.IsFalse(result, "Does_Not_Exist table should not exist in Northwind database");
        }
    }
}