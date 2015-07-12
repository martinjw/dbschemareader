using System;
using System.IO;
using System.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;
using TestCategory = NUnit.Framework.CategoryAttribute;
#endif

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class Firebird
    {
        const string ProviderName = "FirebirdSql.Data.FirebirdClient";
        const string Path = @"C:\Program Files\Firebird\Firebird_2_5\examples\empbuild\EMPLOYEE.FDB";
        const string ConnectionString = "User=SYSDBA;Password=masterkey;Database=" + Path + ";Server=localhost; Connection lifetime=15;Pooling=true";
        private DatabaseReader _dbReader;

        [ClassInitialize]
        public static void Config(TestContext context)
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
            //Act
            var schema = _dbReader.ReadAll();
            //Assert
            Assert.AreEqual(10, schema.Tables.Count, "The example database contains 10 tables.");
            Assert.IsTrue(schema.Views.Any(s => s.Name == "PHONE_LIST"), "The example database contains PHONE_LIST view.");
            Assert.IsTrue(schema.StoredProcedures.Any(s => s.Name == "ADD_EMP_PROJ"), "The example database contains ADD_EMP_PROJ stored procedure.");
            Assert.IsTrue(schema.Functions.Any(f => f.Name == "RDB$GET_CONTEXT"), "The example database contains RDB$GET_CONTEXT function.");
            Assert.IsTrue(schema.Sequences.Any(s => s.Name == "EMP_NO_GEN"), "The example database contains EMP_NO_GEN sequence.");
            var employees = schema.FindTableByName("EMPLOYEE");
            AssertEmployeeTableValid(employees);
            Assert.AreEqual(1, employees.CheckConstraints.Count, "The employee table contains 1 constraint check.");
            Assert.AreEqual(5, employees.ForeignKeyChildren.Count, "The employee table referenced by 5 tables.");
        }

        [TestMethod, TestCategory("Firebird")]
        public void Table_OnEmployeeTable_ShouldReturnTheEmployeeTableInformation()
        {
            var employees = _dbReader.Table("EMPLOYEE");
            AssertEmployeeTableValid(employees);
        }

        private static void AssertEmployeeTableValid(DatabaseTable employees)
        {
            Assert.AreEqual(11, employees.Columns.Count, "The employee table contains 11 columns.");
            Assert.AreEqual(2, employees.ForeignKeys.Count, "The employee table contains 2 foreign keys.");
            var integ28 = employees.ForeignKeys.FirstOrDefault(f => f.Name == "INTEG_28");
            Assert.AreEqual(1, integ28.Columns.Count, "The INTEG_28 fk on employee table references with 1 key.");
            var integ29 = employees.ForeignKeys.FirstOrDefault(f => f.Name == "INTEG_29");
            Assert.AreEqual(3, integ29.Columns.Count, "The INTEG_28 fk on employee table references with 3 key.");
        }

    }
}
