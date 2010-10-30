using DatabaseSchemaReader;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
#endif

namespace DatabaseSchemaReaderTest
{
    
    /// <summary>
    /// INTEGRATION TEST
    ///</summary>
    [TestClass()]
    public class DatabaseReaderTest
    {

        /// <summary>
        ///A test for DatabaseReader Constructor
        ///</summary>
        [TestMethod()]
        public void DatabaseReaderConstructorTest()
        {
            var target = new DatabaseReader();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}
