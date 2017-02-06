using System;

namespace SqlServerSchemaReaderTest
{

    /// <summary>
    /// Common connection strings used in many tests
    /// </summary>
    static class ConnectionStrings
    {
        public static string TestSchema
        {
            get
            {
                if (string.Equals("True", Environment.GetEnvironmentVariable("APPVEYOR")))
                {
                    return @"Server=(local)\SQL2008R2SP2;Database=TestSchema;User ID=sa;Password=Password12!";
                }
                return @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=TestSchema";
            }
        }
    }
}
