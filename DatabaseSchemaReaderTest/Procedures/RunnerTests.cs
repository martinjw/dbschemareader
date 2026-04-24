using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader;

namespace DatabaseSchemaReaderTest.Procedures
{
    /// <summary>
    /// Summary description for RunnerTests
    /// </summary>
    [TestClass]
    public class RunnerTests
    {
        [TestMethod]
        public void TestRunnerWithEmptySchema()
        {
            var connectionString = ConnectionStrings.Northwind;
            DatabaseSchema schema = null;
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var northwindReader = new DatabaseReader(con);
                    northwindReader.Owner = "dbo";
                    schema = northwindReader.DatabaseSchema;
                    var runner = new ResultSetReader(schema);
                    runner.Execute(con);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Could not open Northwind: {e}");
            }

            //should load sprocs
            var sproc = schema.StoredProcedures.First();
            var result = sproc.ResultSets.First();
            var col = result.Columns.First();
            //Assert.IsNotNull(col.DataType, "Should have loaded a datatype");
        }

        [TestMethod]
        public void TestRunnerWithNorthwind()
        {
            //smoke test - does this run without any exceptions
            var connectionString = ConnectionStrings.Northwind;
            DatabaseSchema schema = null;
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var northwindReader = new DatabaseReader(con);
                    northwindReader.Owner = "dbo";
                    schema = northwindReader.ReadAll();
                    var runner = new ResultSetReader(schema);
                    runner.Execute(con);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"Could not open Northwind: {e}");
            }

            var directory = TestHelper.CreateDirectory("NorthwindSproc");
            const string @namespace = "Northwind.Domain";
            var settings = new CodeWriterSettings { Namespace = @namespace };

            var codeWriter = new CodeWriter(schema, settings);
            codeWriter.Execute(directory);

            Debug.WriteLine("Check project in " + directory.FullName);
        }
    }
}
