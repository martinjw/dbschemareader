using DatabaseSchemaReader.Data;
using DatabaseSchemaReaderTest.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.InsertWriterTests
{
    [TestClass]
    public class ScriptWriterTest
    {
        const string Providername = "System.Data.SqlClient";
        readonly string _connectionString = ConnectionStrings.Northwind;


        [TestMethod]
        public void TestInsertIntegration()
        {
            //arrange
            var dbReader = TestHelper.GetNorthwindReader();
            var table = dbReader.Table("Categories");

            var rdr = new ScriptWriter();

            //act
            var txt = rdr.ReadTable(table, _connectionString, Providername);

            //assert
            Assert.IsTrue(txt.Contains("INSERT INTO [Categories]"), "Insert statments created: [" + txt + "]");
            Assert.IsTrue(txt.Contains("[CategoryName],  [Description]"), "Insert names the columns: [" + txt + "]");
            Assert.IsTrue(txt.Contains("'Beverages'"), "Data includes Beverages: [" + txt + "]");
        }

        [TestMethod]
        public void TestInsertNoDirectDatabaseReaderIntegration()
        {
            //arrange
            ProviderChecker.Check(Providername, _connectionString);

            var rdr = new ScriptWriter();

            //act
            var txt = rdr.ReadTable("Categories", _connectionString, Providername);

            //assert
            Assert.IsTrue(txt.Contains("INSERT INTO [Categories]"), "Insert statments created: [" + txt + "]");
            Assert.IsTrue(txt.Contains("[CategoryName],  [Description]"), "Insert names the columns: [" + txt + "]");
            Assert.IsTrue(txt.Contains("'Beverages'"), "Data includes Beverages: [" + txt + "]");
        }

        [TestMethod]
        public void TestInsertWithDataReader()
        {
            //arrange
            var dbReader = TestHelper.GetNorthwindReader();
            dbReader.DataTypes(); //need the datatypes here, so this must be called before or after
            var table = dbReader.Table("Categories");

            var rdr = new ScriptWriter();

            string result = null;

            //act
            rdr.ReadTable(table, _connectionString, Providername, insertString =>
            {
                result = insertString;
                return false; //only need one record, return
            });

            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("INSERT INTO [Categories]"), "Insert statments created: " + result);
            Assert.IsTrue(result.Contains("[CategoryName],"), "Insert names the columns: " + result);
        }
    }
}
