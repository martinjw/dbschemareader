using System;

namespace DatabaseSchemaReaderTest
{

    /// <summary>
    /// Common connection strings used in many tests
    /// </summary>
    static class ConnectionStrings
    {
        public const string OracleHr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=XE)));User Id=hr;Password=hr;";
        //public const string Northwind = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";

        public static string Northwind
        {
            get
            {
                if (string.Equals("True", Environment.GetEnvironmentVariable("APPVEYOR")))
                {
                    return @"Server=(local)\SQL2008R2SP2;Database=NorthwindDsr;User ID=sa;Password=Password12!";
                }
                return @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
            }
        }
    }
}
