using System;
using System.Configuration;
using System.IO;
using System.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;
using Microsoft.Build.Evaluation;

namespace CodeGenTester
{
    class Program
    {
        static void Main()
        {
            const string providername = "System.Data.SqlClient";
            const string connectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";

            Console.WriteLine("Reading Northwind");
            var dr = new DatabaseReader(connectionString, providername);
            var schema = dr.ReadAll();

            Console.WriteLine("Schema read");

            var directory = CreateDirectory();
            Console.WriteLine("Writing to directory " + directory.FullName);

            if (WriteNHibernate(directory, schema) &&
                WriteFluentNHibernate(directory, schema) &&
                WriteCodeFirst(directory, schema) &&
                BuildRunner(directory))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("All done");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Build failed");
                Console.ResetColor();
                Console.ReadKey();
            }

        }

        private static bool WriteNHibernate(DirectoryInfo directory, DatabaseSchema schema)
        {
            var sub = CreateDirectory(directory, "NHib");
            var settings = new CodeWriterSettings
                               {
                                   Namespace = "Northwind.NHib",
                                   CodeTarget = CodeTarget.PocoNHibernateHbm,
                                   WriteProjectFile = true
                               };

            var codeWriter = new CodeWriter(schema, settings);

            codeWriter.Execute(sub);
            Console.WriteLine("Wrote NHib project to " + sub.FullName);
            var isBuilt = BuildProject(Path.Combine(sub.FullName, settings.Namespace + ".csproj"));
            return isBuilt;
        }

        private static bool WriteFluentNHibernate(DirectoryInfo directory, DatabaseSchema schema)
        {
            var sub = CreateDirectory(directory, "FluentNHib");
            var settings = new CodeWriterSettings
            {
                Namespace = "Northwind.FluentNHib",
                CodeTarget = CodeTarget.PocoNHibernateFluent,
                WriteProjectFile = true
            };

            var codeWriter = new CodeWriter(schema, settings);

            codeWriter.Execute(sub);
            Console.WriteLine("Wrote Fluent NHib project to " + sub.FullName);
            var isBuilt = BuildProject(Path.Combine(sub.FullName, settings.Namespace + ".csproj"));
            return isBuilt;
        }

        private static bool WriteCodeFirst(DirectoryInfo directory, DatabaseSchema schema)
        {
            var sub = CreateDirectory(directory, "CodeFirst");
            var settings = new CodeWriterSettings
            {
                Namespace = "Northwind.CodeFirst",
                CodeTarget = CodeTarget.PocoEntityCodeFirst,
                UseForeignKeyIdProperties = true,
                WriteProjectFile = true
            };

            var codeWriter = new CodeWriter(schema, settings);

            codeWriter.Execute(sub);
            Console.WriteLine("Wrote EF CodeFirst project to " + sub.FullName);
            var isBuilt = BuildProject(Path.Combine(sub.FullName, settings.Namespace + ".csproj"));
            return isBuilt;
        }

        private static bool BuildRunner(DirectoryInfo directory)
        {
            var path = Path.Combine(directory.FullName, "CodeGen.TestRunner");
            //looks ok
            if (!Directory.Exists(path)) return true;
            var csproj = Path.Combine(path, "CodeGen.TestRunner.csproj");
            if (!File.Exists(csproj)) return true;
            var isBuilt = BuildProject(csproj);
            if (!isBuilt) return false;
            var dll = Path.Combine(path, @"bin\Debug\CodeGen.TestRunner.exe");

            try
            {
                Console.WriteLine("Executing generating code");
                Console.ForegroundColor = ConsoleColor.Magenta;
                AssemblyRunner.Run(dll, "CodeGen.TestRunner.Runner", "Run");
                Console.ResetColor();
            }
            catch (Exception exception)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(exception.ToString());
                return false;
            }

            return true;
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
            string txt = "Built " + Path.GetFileName(projectPath);
            if (!success && File.Exists(logPath))
                txt = File.ReadAllText(logPath);
            Console.WriteLine(txt);

            return success;
        }

        private static DirectoryInfo CreateDirectory()
        {
            var path = ConfigurationManager.AppSettings["Destination"];

            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                path = Path.Combine(Path.GetTempPath(), "DatabaseSchemaReader");
            }

            var directory = new DirectoryInfo(path);
            return directory;
        }


        private static DirectoryInfo CreateDirectory(DirectoryInfo directory, string folder)
        {
            if (!directory.Exists)
            {
                directory.Create();
            }
            if (directory.GetDirectories(folder).Any())
            {
                //if it's already there, clear it out
                var sub = directory.GetDirectories(folder).First();
                try
                {
                    sub.Delete(true);
                }
                catch (IOException)
                {
                    //retry
                    System.Threading.Thread.Sleep(500);
                    sub.Delete(true);
                }
            }
            var subdirectory = directory.CreateSubdirectory(folder);
            //because it may not actually have been created...
            if (!subdirectory.Exists)
                subdirectory.Create();
            return subdirectory;
        }
    }
}
