using System;
using System.IO;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
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
            const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();
            return schema.FindTableByName("Categories");
        }

        [TestMethod]
        public void TestWritingCrudSprocs()
        {
            var table = LoadCategoriesFromNorthwind();

            //let's create the SQLServer crud procedures
            var gen = new DatabaseSchemaReader.SqlGen.SqlServer.ProcedureGenerator(table);
            gen.ManualPrefix = table.Name + "__";
            var path = Path.Combine(Environment.CurrentDirectory, "sqlserver_sprocs.sql");
            gen.WriteToScript(path);

            //manually check the script is ok
        }



        [TestMethod]
        public void TestWritingCrudSprocsWithOracleConversion()
        {
            var table = LoadCategoriesFromNorthwind();

            //let's pretend it's an oracle table and create an oracle package
            var oracleGen = new DatabaseSchemaReader.SqlGen.Oracle.ProcedureGenerator(table);
            oracleGen.ManualPrefix = table.Name + "__";
            //here i want all my parameters prefixed by a p
            oracleGen.FormatParameter = name => "p_" + name;
            //also define the cursor parameter
            oracleGen.CursorParameterName = "p_cursor";
            var oraclePath = Path.Combine(Environment.CurrentDirectory, "oracle_sprocs.sql");
            oracleGen.WriteToScript(oraclePath);

            //manually check the script is ok
        }
    }
}
