using System.Diagnostics;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.Procedures;
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

namespace DatabaseSchemaReaderTest.Procedures
{
    /// <summary>
    /// Summary description for RunnerTests
    /// </summary>
    [TestClass]
    public class RunnerTests
    {

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
