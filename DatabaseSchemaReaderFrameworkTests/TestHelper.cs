using DatabaseSchemaReader;

namespace DatabaseSchemaReaderFrameworkTests
{
    internal static class TestHelper
    {
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