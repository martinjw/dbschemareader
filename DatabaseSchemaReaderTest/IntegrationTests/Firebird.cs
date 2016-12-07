using System.IO;
using System.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class Firebird
    {
        // IMPORTANT: Path is for a 32 bit install of Firebird 3 (default example database) with default user/password
        // In Firebird.conf, set WireCrypt = Enabled (.net provider doesn't support the default required mode)

        const string ProviderName = "FirebirdSql.Data.FirebirdClient";
        const string Path = @"C:\Program Files (x86)\Firebird\Firebird_3_0\examples\empbuild\EMPLOYEE.FDB";
        const string ConnectionString = "User=SYSDBA;Password=masterkey;Database=" + Path + ";Server=localhost; Connection lifetime=15;Pooling=true";
        private DatabaseReader _dbReader;

        [ClassInitialize]
        public static void Config(TestContext context)
        {
            //nuget install FirebirdSql.Data.FirebirdClient
            //  <system.data>
            //    <DbProviderFactories>
            //      <add name="FirebirdClient Data Provider" invariant="FirebirdSql.Data.FirebirdClient"
            //           description = ".NET Framework Data Provider for Firebird" 
            //           type = "FirebirdSql.Data.FirebirdClient.FirebirdClientFactory, FirebirdSql.Data.FirebirdClient" />
            //    </DbProviderFactories>
            //  </system.data>

            ProviderChecker.Check(ProviderName, ConnectionString);


        }
        [TestInitialize]
        public void Setup()
        {
            _dbReader = new DatabaseReader(ConnectionString, ProviderName);
        }

        [TestMethod, TestCategory("Firebird")]
        public void ReadAll_OnFirebirdEmployeeDatabase_ShouldReturnTheCompleteSchema()
        {
            if (!File.Exists(Path))
            {
                Assert.Inconclusive("Firebird example database not found " + Path);
            }
            //Act
            var schema = _dbReader.ReadAll();
            //Assert
            Assert.AreEqual(10, schema.Tables.Count, "The example database contains 10 tables.");
            Assert.IsTrue(schema.Views.Any(s => s.Name == "PHONE_LIST"), "The example database contains PHONE_LIST view.");
            Assert.IsTrue(schema.StoredProcedures.Any(s => s.Name == "ADD_EMP_PROJ"), "The example database contains ADD_EMP_PROJ stored procedure.");
            //Assert.IsTrue(schema.Functions.Any(f => f.Name == "RDB$GET_CONTEXT"), "The example database contains RDB$GET_CONTEXT function.");
            Assert.IsTrue(schema.Sequences.Any(s => s.Name == "EMP_NO_GEN"), "The example database contains EMP_NO_GEN sequence.");
            var employees = schema.FindTableByName("EMPLOYEE");
            AssertEmployeeTableValid(employees);
            Assert.AreEqual(1, employees.CheckConstraints.Count, "The employee table contains 1 constraint check.");
            Assert.AreEqual(5, employees.ForeignKeyChildren.Count, "The employee table referenced by 5 tables.");
        }

        [TestMethod, TestCategory("Firebird")]
        public void Table_OnEmployeeTable_ShouldReturnTheEmployeeTableInformation()
        {
            if (!File.Exists(Path))
            {
                Assert.Inconclusive("Firebird example database not found " + Path);
            }
            var employees = _dbReader.Table("EMPLOYEE");
            AssertEmployeeTableValid(employees);
        }

        private static void AssertEmployeeTableValid(DatabaseTable employees)
        {
            Assert.AreEqual(11, employees.Columns.Count, "The employee table contains 11 columns.");
            Assert.AreEqual(2, employees.ForeignKeys.Count, "The employee table contains 2 foreign keys.");
            var integ28 = employees.ForeignKeys.First(f => f.Name == "INTEG_28");
            Assert.AreEqual(1, integ28.Columns.Count, "The INTEG_28 fk on employee table references with 1 key.");
            var integ29 = employees.ForeignKeys.First(f => f.Name == "INTEG_29");
            Assert.AreEqual(3, integ29.Columns.Count, "The INTEG_28 fk on employee table references with 3 key.");
        }

    }
}
