using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// The following databases should exist on localhost:
    ///     Postgresql with World sample database (userId postgres, password sql)
    /// Via Npgsql data provider (which has Users, Tables, Views and Columns only- very limited)
    /// </summary>
    [TestClass]
    public class Postgresql
    {
        [TestMethod, TestCategory("Postgresql")]
        public void TestNpgsql()
        {
            //using the MySql world database ported to Postgres
            DatabaseSchema schema = null;
            var connectionString = ConnectionStrings.PostgreSql;

            try
            {
                using (var con = new Npgsql.NpgsqlConnection(connectionString))
                {
                    var dbReader = new DatabaseReader(con);
                    dbReader.Owner = "public"; //otherwise you have "postgres" owned tables and views
                    schema = dbReader.ReadAll();
                }
            }
            catch (Exception)
            {
                Assert.Inconclusive();
            }

            var country = schema.FindTableByName("country");
            Assert.IsTrue(country.Columns.Count > 0);

            //reading index with columns in correct order
            var rental = schema.FindTableByName("rental");
            var idx = rental.Indexes.Find(x =>
                x.Name == "idx_unq_rental_rental_date_inventory_id_customer_id");
            Assert.IsTrue(idx.IsUnique);
            Assert.AreEqual(3, idx.Columns.Count);
            Assert.AreEqual("rental_date", idx.Columns[0].Name);
            Assert.AreEqual("inventory_id", idx.Columns[1].Name);
            Assert.AreEqual("customer_id", idx.Columns[2].Name);

            //reading a domain
            var year = schema.UserDataTypes.Find(x => x.Name == "year");
            Assert.IsNotNull(year, "DOMAIN year should be defined");
            Assert.IsTrue(year.DataType.IsInt, "Underlying data type is INT");
        }
    }
}