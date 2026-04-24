using DatabaseSchemaReader;
using DatabaseSchemaReader.Data;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace DatabaseSchemaReaderTest.SqlGen.InsertWriterTests
{
    [TestClass]
    public class ScriptWriterTest
    {
        private string ScriptWriterRunner()
        {
            var connectionString = ConnectionStrings.Northwind;
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();

                    var northwindReader = new DatabaseReader(con);
                    northwindReader.Owner = "dbo";
                    var schema = northwindReader.ReadAll();

                    var table = northwindReader.Table("Categories");
                    var rdr = new ScriptWriter();
                    return rdr.ReadTable(table, con);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Could not open Northwind: {e}");
                return null;
            }
        }

        [TestMethod]
        public void TestInsertIntegration()
        {
            //act
            var txt = ScriptWriterRunner();

            //assert
            Assert.IsTrue(txt.Contains("INSERT INTO [Categories]"), "Insert statments created: [" + txt + "]");
            Assert.IsTrue(txt.Contains("[CategoryName],  [Description]"), "Insert names the columns: [" + txt + "]");
            Assert.IsTrue(txt.Contains("'Beverages'"), "Data includes Beverages: [" + txt + "]");
        }

        [TestMethod]
        public void TestInsertNoDirectDatabaseReaderIntegration()
        {
            //act
            var txt = ScriptWriterRunner();

            //assert
            Assert.IsTrue(txt.Contains("INSERT INTO [Categories]"), "Insert statments created: [" + txt + "]");
            Assert.IsTrue(txt.Contains("[CategoryName],  [Description]"), "Insert names the columns: [" + txt + "]");
            Assert.IsTrue(txt.Contains("'Beverages'"), "Data includes Beverages: [" + txt + "]");
        }

        [TestMethod]
        public void TestInsertWithDataReader()
        {
            //arrange
            string result = null;

            //act
            var connectionString = ConnectionStrings.Northwind;
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();

                    var northwindReader = new DatabaseReader(con);
                    northwindReader.Owner = "dbo";
                    //need the datatypes here, so this must be called before or after
                    northwindReader.DataTypes();

                    var table = northwindReader.Table("Categories");
                    var rdr = new ScriptWriter();
                    rdr.ReadTable(table, con, insertString =>
                    {
                        result = insertString;
                        return false; //only need one record, return
                    });
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Could not open Northwind: {e}");
                return;
            }

            //assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("INSERT INTO [Categories]"), "Insert statments created: " + result);
            Assert.IsTrue(result.Contains("[CategoryName],"), "Insert names the columns: " + result);
        }
    }
}