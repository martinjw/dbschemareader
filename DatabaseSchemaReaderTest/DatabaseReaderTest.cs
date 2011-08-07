using System;
using DatabaseSchemaReader;
#if !NUNIT
using DatabaseSchemaReader.DataSchema;
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
    /// 
    ///</summary>
    [TestClass]
    public class DatabaseReaderTest
    {

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoConnectionStringTest()
        {
            new DatabaseReader(null, SqlType.SqlServer);

            Assert.Fail("Should not have succeeded");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NoProviderTest()
        {
            new DatabaseReader("Dummy", null);

            Assert.Fail("Should not have succeeded");
        }

        [TestMethod]
        public void SqlTypeTest()
        {
            var dr =  new DatabaseReader("Dummy", SqlType.SqlServer);
            Assert.AreEqual("System.Data.SqlClient", dr.DatabaseSchema.Provider);

            //the other types will fail if they aren't installed
        }
    }
}
