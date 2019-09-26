using System;
using System.IO;

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
                    return @"Server=(local)\SQL2017;Database=NorthwindDsr;User ID=sa;Password=Password12!";
                }
                return @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";
            }
        }

        public static string MySql
        {
            get
            {
                return MySqlDevart + "Allow User Variables=True;";
            }
        }

        public static string MySqlDevart
        {
            get
            {
                if (string.Equals("True", Environment.GetEnvironmentVariable("APPVEYOR")))
                {
                    return @"Server=localhost;Uid=root;Pwd=Password12!;Database=sakila;";
                }
                return @"Server=localhost;Uid=root;Pwd=mysql;Database=sakila;";
            }
        }

        public static string PostgreSql
        {
            get
            {
                if (string.Equals("True", Environment.GetEnvironmentVariable("APPVEYOR")))
                {
                    return @"Server=127.0.0.1;User id=postgres;Pwd=Password12!;database=world;";
                }
                return @"Server=127.0.0.1;User id=postgres;password=sql;database=world;";
            }
        }

        public static string SqLiteFilePath
        {
            get { return Path.Combine(Environment.CurrentDirectory, "northwind.db"); }
        }

        public static string SqlServerCeFilePath
        {
            get { return Path.Combine(Environment.CurrentDirectory, "northwind.sdf"); }
        }
    }
}
