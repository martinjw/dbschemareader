using System.IO;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReaderTest.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen
{
    /// <summary>
    /// Take a table and write CRUD sprocs
    /// </summary>
    [TestClass]
    public class WritingSprocsTest
    {

        private static DatabaseTable LoadCategoriesFromNorthwind()
        {
            const string providername = "System.Data.SqlClient";
            var connectionString = ConnectionStrings.Northwind;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            return schema.FindTableByName("Categories");
        }


        [TestMethod]
        public void TestWritingNorthwindTables()
        {
            var schemas = LoadCategoriesFromNorthwind().DatabaseSchema;

            //take a SQLServer Northwind and write all the tables and relations
            var gen = new DdlGeneratorFactory(SqlType.SqlServer).AllTablesGenerator(schemas);
            var txt = gen.Write();

            Assert.IsFalse(string.IsNullOrEmpty(txt), "Should have written some text");

            //let's translate it into Oracle.
            var oracleGen = new DdlGeneratorFactory(SqlType.Oracle).AllTablesGenerator(schemas);
            oracleGen.IncludeSchema = false; //we don't want "dbo." prefixes
            txt = oracleGen.Write();
            Assert.IsFalse(string.IsNullOrEmpty(txt), "Should have written some text");

            //let's translate it into MySQL.
            var mysqlGen = new DdlGeneratorFactory(SqlType.MySql).AllTablesGenerator(schemas);
            mysqlGen.IncludeSchema = false; //we don't want "dbo." prefixes
            var mySqlTxt = mysqlGen.Write();
            Assert.IsFalse(string.IsNullOrEmpty(mySqlTxt), "Should have written some text");

            //let's translate it into SQLite.
            var sqliteGen = new DdlGeneratorFactory(SqlType.SQLite).AllTablesGenerator(schemas);
            sqliteGen.IncludeSchema = false; //we don't want "dbo." prefixes
            var sqliteTxt = sqliteGen.Write();
            Assert.IsFalse(string.IsNullOrEmpty(sqliteTxt), "Should have written some text");

            //manually check the script is ok
        }

        [TestMethod]
        public void TestWritingSqlTableIntoOracleTable()
        {
            var table = LoadCategoriesFromNorthwind();

            //take a SQLServer table and create Oracle table DDL
            var gen = new DdlGeneratorFactory(SqlType.Oracle).TableGenerator(table);

            var txt = gen.Write();

            Assert.IsFalse(string.IsNullOrEmpty(txt), "Should have written some text");

            //manually check the script is ok
        }


        [TestMethod]
        public void TestWritingSqlTableIntoMySqlTable()
        {
            var table = LoadCategoriesFromNorthwind();

            //take a SQLServer table and create MySQL table DDL
            var gen = new DdlGeneratorFactory(SqlType.MySql).TableGenerator(table);

            var txt = gen.Write();

            Assert.IsFalse(string.IsNullOrEmpty(txt), "Should have written some text");

            //manually check the script is ok
        }

        [TestMethod]
        public void TestWritingCrudSprocs()
        {
            var table = LoadCategoriesFromNorthwind();

            //let's create the SQLServer crud procedures
            var gen = new DdlGeneratorFactory(SqlType.SqlServer).ProcedureGenerator(table);
            gen.ManualPrefix = table.Name + "__";
            var destination = TestHelper.CreateDirectory("sql").FullName;
            var path = Path.Combine(destination, "sqlserver_sprocs.sql");
            gen.WriteToScript(path);

            var txt = File.ReadAllText(path);
            Assert.IsFalse(string.IsNullOrEmpty(txt), "Should have written some text");
            //manually check the script is ok
        }



        [TestMethod]
        public void TestWritingCrudSprocsWithOracleConversion()
        {
            var table = LoadCategoriesFromNorthwind();

            //let's pretend it's an oracle table and create an oracle package
            var oracleGen = new DdlGeneratorFactory(SqlType.Oracle).ProcedureGenerator(table);
            oracleGen.ManualPrefix = table.Name + "__";
            //here i want all my parameters prefixed by a p
            oracleGen.FormatParameter = name => "p_" + name;
            //also define the cursor parameter
            oracleGen.CursorParameterName = "p_cursor";
            var destination = TestHelper.CreateDirectory("sql").FullName;
            var oraclePath = Path.Combine(destination, "oracle_sprocs.sql");
            oracleGen.WriteToScript(oraclePath);

            var txt = File.ReadAllText(oraclePath);
            Assert.IsFalse(string.IsNullOrEmpty(txt), "Should have written some text");
            //manually check the script is ok
        }
    }
}
