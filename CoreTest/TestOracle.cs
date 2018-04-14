using System.Data.Common;
using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oracle.ManagedDataAccess.Client;

namespace CoreTest
{
    [TestClass]
    public class TestOracle
    {
        //using <PackageReference Include="Oracle.ManagedDataAccess.Core" Version="2.12.0-beta2" />
        public const string OracleHr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=XE)));User Id=hr;Password=hr;";

        [TestMethod, TestCategory("Oracle")]
        public void RunOracleSchema()
        {
            try
            {
                using (var connection = new OracleConnection(OracleHr))
                {
                    connection.Open();

                    var databaseReader = new DatabaseReader(connection) {Owner = "HR"};
                    var schema = databaseReader.ReadAll();
                    var employees = schema.FindTableByName("EMPLOYEES");
                
                    Assert.IsNotNull(employees);
                }
            }
            catch (DbException e)
            {
                Assert.Inconclusive($"No Oracle database with this connection string? {e}");
            }
        }
    }
}