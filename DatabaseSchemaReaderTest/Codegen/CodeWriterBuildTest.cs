using System;
using System.IO;
using DatabaseSchemaReader.CodeGen;
using Microsoft.Build.Evaluation; //reference Microsoft.Build.dll v4.0 and Microsoft.Build.Framework.dll
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Codegen
{


    /// <summary>
    ///Create a simple model and write it to filesystem
    ///</summary>
    [TestClass]
    public class CodeWriterBuildTest
    {

        [TestMethod]
        public void BuildGeneratedCodeTest()
        {
            //arrange
            var dbReader = TestHelper.GetNorthwindReader();
            var schema = dbReader.ReadAll();

            var directory = TestHelper.CreateDirectory("NorthwindCodeGen");
            const string @namespace = "Northwind.Domain";
            var settings = new CodeWriterSettings
                               {
                                   Namespace = @namespace, 
                                   CodeTarget = CodeTarget.PocoNHibernateHbm, 
                                   Namer = new PluralizingNamer(),
                                   WriteProjectFile = true
                               };

            var codeWriter = new CodeWriter(schema, settings);

            //act
            codeWriter.Execute(directory);

            //assert
            var csproj = Path.Combine(directory.FullName, "Northwind.Domain.csproj");
            Assert.IsTrue(File.Exists(csproj));

            //can we build it?
            var projectIsBuilt = BuildProject(csproj);
            Assert.IsTrue(projectIsBuilt); //yes we can
        }

        /// <summary>
        /// Builds the project - based on http://msdn.microsoft.com/en-us/library/microsoft.build.buildengine.engine.aspx.
        /// </summary>
        /// <param name="projectPath">The project (csproj) path</param>
        /// <returns>True if builds okay</returns>
        private static bool BuildProject(string projectPath)
        {
            var logPath = Path.Combine(Path.GetDirectoryName(projectPath), "build.log");

            //.Net 4 Microsoft.Build.Evaluation.Project and ProjectCollection
            var engine = new ProjectCollection();

            // Instantiate a new FileLogger to generate build log
            var logger = new Microsoft.Build.Logging.FileLogger();

            // Set the logfile parameter to indicate the log destination
            logger.Parameters = @"logfile=" + logPath;

            // Register the logger with the engine
            engine.RegisterLogger(logger);

            // Build a project file
            bool success = engine.LoadProject(projectPath).Build();
            //Unregister all loggers to close the log file
            engine.UnregisterAllLoggers();

            //if fails, put the log file into the assert statement
            string txt = "Should have built";
            if (!success && File.Exists(logPath))
                txt = File.ReadAllText(logPath);
            Console.WriteLine(txt);

            return success;
        }

    }
}
