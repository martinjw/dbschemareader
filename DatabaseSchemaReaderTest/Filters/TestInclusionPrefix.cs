using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseSchemaReader.Filters;

namespace DatabaseSchemaReaderTest.Filters
{
    [TestClass]
    public class TestInclusionPrefix
    {
        [TestMethod]
        public void TestMethod1()
        {
            var filter = new InclusionPrefixFilter("Export", "Import");

            Assert.IsTrue(filter.Exclude("Employees"));
            Assert.IsFalse(filter.Exclude("ImportData"));
            Assert.IsFalse(filter.Exclude("ExportData"));
        }
    }
}
