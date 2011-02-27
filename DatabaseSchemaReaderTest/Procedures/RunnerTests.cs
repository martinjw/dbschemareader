using System;
using System.IO;
using System.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.Procedures;
using DatabaseSchemaReaderTest.IntegrationTests;
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
        private static DatabaseReader GetNortwindReader()
        {
            const string providername = "System.Data.SqlClient";
            const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
            ProviderChecker.Check(providername, connectionString);

            var databaseReader = new DatabaseReader(connectionString, providername);
            return databaseReader;
        }

        private static DirectoryInfo CreateDirectory(string folder)
        {
            var directory = new DirectoryInfo(Environment.CurrentDirectory);
            if (directory.GetDirectories(folder).Any())
            {
                //if it's already there, clear it out
                var sub = directory.GetDirectories(folder).First();
                sub.Delete(true);
            }
            return directory.CreateSubdirectory(folder);
        }

        [TestMethod]
        public void TestRunnerWithNorthwind()
        {
            var dbReader = GetNortwindReader();
            var schema = dbReader.ReadAll();

            var runner = new ResultSetReader(schema);
            runner.Execute();

            var directory = CreateDirectory("NorthwindSproc");
            const string @namespace = "Northwind.Domain";

            var codeWriter = new CodeWriter(schema);
            codeWriter.Execute(directory, @namespace);
        }
    }
}
