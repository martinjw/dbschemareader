namespace DatabaseSchemaReaderTest
{

    /// <summary>
    /// Common connection strings used in many tests
    /// </summary>
    static class ConnectionStrings
    {
        public const string OracleHr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SID=XE)));User Id=hr;Password=hr;";
        public const string Northwind = @"Data Source=.\SQLEXPRESS;Integrated Security=true;Initial Catalog=Northwind";

    }
}
