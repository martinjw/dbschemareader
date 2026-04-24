using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DatabaseSchemaReaderTest
{
    internal static class TestHelper
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

        public static DatabaseSchema GetNorthwindSchema()
        {
            DatabaseSchema schema = null;
            if (!GetNorthwindReader(reader => schema = reader.ReadAll()))
            {
                return null;
            }
            return schema;
        }

        /// <summary>
        /// Gets the SqlServer NorthWind reader.
        /// </summary>
        /// <returns></returns>
        public static bool GetNorthwindReader(Action<DatabaseReader> configure)
        {
            var connectionString = ConnectionStrings.Northwind;
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    var northwindReader = new DatabaseReader(con);
                    northwindReader.Owner = "dbo";
                    configure.Invoke(northwindReader);
                }

                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError($"Could not open Northwind: {e}");
                return false;
            }
        }
    }
}