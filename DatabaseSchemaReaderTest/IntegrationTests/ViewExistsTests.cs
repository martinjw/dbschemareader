using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class ViewExistsTests
    {
        [TestMethod, TestCategory("SqlServer")]
        public void ViewExists()
        {
            //arrange
            var dbReader = TestHelper.GetNorthwindReader();

            //act
            var views = dbReader.AllViews();
            foreach (var view in views)
            {
                var result = dbReader.ViewExists(view.Name);
                //assert
                Assert.IsTrue(result, $"View {view.Name} should exist in Northwind database");
            }
        }

        [TestMethod, TestCategory("SqlServer")]
        public void ViewDoesNotExist()
        {
            //arrange
            var dbReader = TestHelper.GetNorthwindReader();
            //act
            var result = dbReader.ViewExists("Does_Not_Exist");
            //assert
            Assert.IsFalse(result, "Does_Not_Exist view should not exist in Northwind database");
        }
    }
}