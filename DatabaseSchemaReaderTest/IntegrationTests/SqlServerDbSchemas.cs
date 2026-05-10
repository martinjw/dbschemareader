using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class SqlServerDbSchemas
    {
        [TestMethod, TestCategory("SqlServer")]
        public void DiscoverSqlServerDbSchemas()
        {
            IList<DatabaseDbSchema> schemas = null;
            if (!TestHelper.GetNorthwindReader(reader => schemas = reader.AllSchemas()))
            {
                return;
            }

            var dbo = schemas.FirstOrDefault(x => x.Name == "dbo");
            Assert.IsNotNull(dbo);
        }
    }
}