using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Procedures;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var dbReader = TestHelper.GetNorthwindReader();
            
            var schema = dbReader.DatabaseSchema;
            var runner = new ResultSetReader(schema);
            runner.Execute();
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
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();

            var runner = new ResultSetReader(schema);
            runner.Execute();

            var directory = TestHelper.CreateDirectory("NorthwindSproc");
            const string @namespace = "Northwind.Domain";
            var settings = new CodeWriterSettings { Namespace = @namespace };

            var codeWriter = new CodeWriter(schema, settings);
            codeWriter.Execute(directory);

            Debug.WriteLine("Check project in " + directory.FullName);
        }
    }
}
