using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// The following databases should exist on localhost:
    ///     Postgresql with World sample database (userId HR, password HR)
    /// Via Npgsql data provider (which has Users, Tables, Views and Columns only- very limited)
    /// </summary>
    [TestClass]
    public class Postgresql
    {
        //You must have the configuration set up
        //	<system.data>
        //		<DbProviderFactories>
        //			<remove invariant="Npgsql" />
        //			<add name="Npgsql Data Provider" invariant="Npgsql"
        //				 description=".Net Framework Data Provider for Postgresql Server"
        //				 type="Npgsql.NpgsqlFactory, Npgsql, Version=2.0.10.0, Culture=neutral, PublicKeyToken=5d8b90d52f46fda7"/>
        //		</DbProviderFactories>
        //	</system.data>
        //also reference Npgsql.dll and Mono.Security.dll

        //[TestMethod]
        //public void TestNpgsql()
        //{
        //    //using the MySql world database ported to Postgres
        //    const string providername = "Npgsql";
        //    const string connectionString = @"Server=127.0.0.1;User id=postgres;password=secret;database=World;";

        //    var dbReader = new DatabaseReader(connectionString, providername);
        //    var schema = dbReader.ReadAll();
        //    var orders = schema.FindTableByName("country");
        //    Assert.AreEqual(15, orders.Columns.Count);
        //}
    }
}
