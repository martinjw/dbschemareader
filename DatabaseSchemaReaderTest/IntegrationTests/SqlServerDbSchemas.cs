using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class SqlServerDbSchemas
    {
        [TestMethod, TestCategory("SqlServer")]
        public void DiscoverSqlServerDbSchemas()
        {
            var dbReader = TestHelper.GetNorthwindReader();
            var schemas = dbReader.AllSchemas();

            var dbo = schemas.FirstOrDefault(x => x.Name == "dbo");
            Assert.IsNotNull(dbo);
        }
    }
}