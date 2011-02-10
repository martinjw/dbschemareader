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

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class Firebird
    {
        [TestMethod]
        public void FirebirdTest()
        {
            //  <system.data>
            //    <DbProviderFactories>
            //      <add
            //          name="Firebird Data Provider"
            //          invariant="FirebirdSql.Data.FirebirdClient" description="Firebird"
            //          type="FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient, Version=2.5.2.0, Culture=neutral, PublicKeyToken=3750abcc3150b00c"
            //      />
            //    </DbProviderFactories>
            //  </system.data>
            const string providername = "FirebirdSql.Data.FirebirdClient";
            const string path = @"C:\Program Files\Firebird\Firebird_2_1\examples\empbuild\EMPLOYEE.FDB";
            const string connectionString = "User=SYSDBA;Password=masterkey;Database=" + path + ";Server=localhost; Connection lifetime=15;Pooling=true";

            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var employees = schema.FindTableByName("EMPLOYEE");
            Assert.AreEqual(11, employees.Columns.Count);

            var table = dbReader.Table("EMPLOYEE");
            Assert.AreEqual(11, table.Columns.Count);
        }
    }
}
