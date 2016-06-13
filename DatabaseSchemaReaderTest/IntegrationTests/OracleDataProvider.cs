using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// The following databases should exist on localhost:
    ///     Oracle Express with HR (userId HR, password HR)
    /// </summary>
    [TestClass]
    public class OracleDataProvider
    {
        [TestMethod, TestCategory("Oracle")]
        public void OracleOdp()
        {
            const string providername = "Oracle.DataAccess.Client";
            const string connectionString = ConnectionStrings.OracleHr;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            dbReader.Owner = "HR";
            var schema = dbReader.ReadAll();
            var employees = schema.FindTableByName("EMPLOYEES");
            Assert.AreEqual(11, employees.Columns.Count);

            var table = dbReader.Table("EMPLOYEES");
            Assert.AreEqual(11, table.Columns.Count);
        }

        [TestMethod, TestCategory("Oracle")]
        public void OracleManagedClient()
        {
            //tested using
            //<configuration>
            //  <system.data>
            //    <DbProviderFactories>
            //      <remove invariant="Oracle.ManagedDataAccess.Client" />
            //      <add name="ODP.NET, Managed Driver"
            //           invariant="Oracle.ManagedDataAccess.Client"
            //           description="Oracle Data Provider for .NET, Managed Driver"
            //           type="Oracle.ManagedDataAccess.Client.OracleClientFactory, Oracle.ManagedDataAccess, Version=4.121.1.0, Culture=neutral, PublicKeyToken=89b483f429c47342" />
            //    </DbProviderFactories>
            //  </system.data>
            //</configuration>

            const string providername = "Oracle.ManagedDataAccess.Client";
            const string connectionString = ConnectionStrings.OracleHr;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            dbReader.Owner = "HR";
            var schema = dbReader.ReadAll();
            var employees = schema.FindTableByName("EMPLOYEES");
            Assert.AreEqual(11, employees.Columns.Count);

            var table = dbReader.Table("EMPLOYEES");
            Assert.AreEqual(11, table.Columns.Count);
        }
    }
}
