using DatabaseSchemaReader;
using DatabaseSchemaReader.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{

    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// The following databases should exist on localhost:
    ///     SqlExpress with Adventureworks (integrated security)
    ///     Oracle Express with HR (userId HR, password HR)
    /// This can also test the DataDirect and Devart drivers.
    ///</summary>
    [TestClass]
    public class DatabaseReaderTest
    {


        [TestMethod, TestCategory("Oracle")]
        public void OleDb()
        {
            const string providername = "System.Data.OleDb";
            const string connectionString = "Provider=msdaora;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=XE)));User Id=hr;Password=hr;";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            //exclude a lot of system sprocs that get picked up. This speeds us up massively.
            dbReader.Exclusions.StoredProcedureFilter = new PrefixFilter("APEX", "ANY", "AGGR", "AQ$", "BLAST",
                "CTX", "DBMS", "DM_", "DBU", "DEVELOP", "DICT", "DIV", "DIU", "DRI", "DRV", "HTF",
                "FTP", "FUNCSTATS", "HS$", "HH", "HTML", "HTP", "HTTP", "KUP", "LCR", "MVAGG",
                "ODC", "ODM", "OGC", "OLAP", "ORA", "OUTLN", "OWA", "PLIT", "PRIVAT", "PRVT",
                "RE$", "SCHEDULER$", "SDO", "SERVER_", "SQL_", "ST_", "STANDARD", "SYS_", "TBLAST", "TFM", "TRANSFORM_",
                "URI", "UTL_", "WPG_", "WPIUTL", "WRI$", "WWV", "XDB", "XML");
            dbReader.Owner = "HR";
            var schema = dbReader.ReadAll();
            var employees = schema.FindTableByName("EMPLOYEES");
            Assert.AreEqual(11, employees.Columns.Count);

            var table = dbReader.Table("EMPLOYEES");
            Assert.AreEqual(11, table.Columns.Count);
        }


        [TestMethod, TestCategory("Oracle")]
        public void Oracle()
        {
            const string providername = "System.Data.OracleClient";
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

        [TestMethod, TestCategory("SqlServer.AdventureWorks")]
        public void SqlServerAdventureWorks()
        {
            const string providername = "System.Data.SqlClient";
            const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=AdventureWorks";
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            var product = schema.FindTableByName("Product");
            Assert.IsTrue(product.Columns.Count >= 17); //different versions of AdventureWorks have different # columns

            var table = dbReader.Table("Product");
            Assert.IsTrue(table.Columns.Count >= 17);
        }

    }
}
