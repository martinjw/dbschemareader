using DatabaseSchemaReader;
using DatabaseSchemaReader.Utilities.DbProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// The following databases should exist on localhost:
    ///     MySQL with sakila (user id root, passwod mysql)
    /// </summary>
    [TestClass]
    public class MySql
    {
        [TestMethod, TestCategory("MySql")]
        public void MySqlTest()
        {
            const string providername = "MySql.Data.MySqlClient";
            var connectionString = ConnectionStrings.MySql;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            dbReader.Owner = "sakila";
            var schema = dbReader.ReadAll();
            var country = schema.FindTableByName("country");
            Assert.AreEqual(3, country.Columns.Count);
            Assert.IsNotNull(country.PrimaryKeyColumn);

            var table = dbReader.Table("city");
            Assert.AreEqual(4, table.Columns.Count);
        }

        [TestMethod, TestCategory("MySql")]
        public void MySqlSchemasTest()
        {
            const string providername = "MySql.Data.MySqlClient";
            var connectionString = ConnectionStrings.MySql;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            dbReader.Owner = "sakila";
            var schemas = dbReader.AllSchemas();
            Assert.IsTrue(schemas.Count > 0, "Schemas should contain sakila");
        }

        [TestMethod, TestCategory("MySql")]
        public void MySqlTableTest()
        {
            const string providername = "MySql.Data.MySqlClient";
            var connectionString = ConnectionStrings.MySql;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var country = dbReader.Table("country");
            Assert.AreEqual(3, country.Columns.Count);
            Assert.IsNotNull(country.PrimaryKeyColumn);
            Assert.IsTrue(country.FindColumn("country_id").IsPrimaryKey);
        }

        [TestMethod, TestCategory("MySql")]
        public void MySqlUnsignedIntegersTest()
        {
            const string providername = "MySql.Data.MySqlClient";
            var connectionString = ConnectionStrings.MySql;
            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();


            var country = schema.FindTableByName("country");

            var pk = country.PrimaryKeyColumn;
            Assert.IsNotNull(pk, "Primary key constraints should be loaded");
            //Assert.AreEqual("smallint(5) unsigned", pk.DbDataType); //not on my laptop db
            Assert.AreEqual("SMALLINT", pk.DataType.TypeName);

        }

        [TestMethod, TestCategory("MySql.Devart")]
        public void MySqlViaDevartTest()
        {
            const string providername = "Devart.Data.MySql";
            var connectionString = ConnectionStrings.MySqlDevart;
            ProviderChecker.Check(providername, connectionString);

            DiscoverProviderFactory.Discover(connectionString, providername);
            var dbReader = new DatabaseReader(connectionString, providername);
            dbReader.Owner = "sakila";
            var schema = dbReader.ReadAll();
            var country = schema.FindTableByName("country");
            Assert.AreEqual(3, country.Columns.Count);
            Assert.IsNotNull(country.PrimaryKeyColumn);

            var table = dbReader.Table("city");
            Assert.AreEqual(4, table.Columns.Count);
        }
    }
}
