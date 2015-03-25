using System;
using System.IO;
using System.Linq;
using DatabaseSchemaReader;
using DatabaseSchemaReaderTest.IntegrationTests;

namespace DatabaseSchemaReaderTest
{
    static class TestHelper
    {

        /// <summary>
        /// Creates the directory for writing test files. We use the %TEMP% directory here.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <returns></returns>
        public static DirectoryInfo CreateDirectory(string folder)
        {
            var path = //Environment.CurrentDirectory;
                Path.Combine(Path.GetTempPath(), "DatabaseSchemaReader");
            var directory = new DirectoryInfo(path);
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
                catch (UnauthorizedAccessException)
                {
                    //can't access it, carry on
                }
            }
            var subdirectory = directory.CreateSubdirectory(folder);
            //because it may not actually have been created...
            if (!subdirectory.Exists)
                subdirectory.Create();
            return subdirectory;
        }

        /// <summary>
        /// Gets the SqlServer NorthWind reader.
        /// </summary>
        /// <returns></returns>
        public static DatabaseReader GetNorthwindReader()
        {
            const string providername = "System.Data.SqlClient";
            var connectionString = ConnectionStrings.Northwind;
            ProviderChecker.Check(providername, connectionString);

            var northwindReader = new DatabaseReader(connectionString, providername);
            northwindReader.Owner = "dbo";
            return northwindReader;
        }
    }
}
