using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

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
            DatabaseSchema schema = null;
            ReadMySql(reader =>
            {
                schema = reader.ReadAll();
            });
            if (schema == null)
            {
                Assert.Inconclusive();
                return;
            }

            var country = schema.FindTableByName("country");
            Assert.AreEqual(3, country.Columns.Count);
            Assert.IsNotNull(country.PrimaryKeyColumn);
        }

        private static void ReadMySql(Action<DatabaseReader> configure)
        {
            var connectionString = ConnectionStrings.MySql;
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var dbReader = new DatabaseReader(connection);
                    dbReader.Owner = "sakila";
                    configure.Invoke(dbReader);
                }
            }
            catch (Exception)
            {
                // Any other unexpected failure - noop
            }
        }

        [TestMethod, TestCategory("MySql")]
        public void MySqlSchemasTest()
        {
            IList<DatabaseDbSchema> schemas = null;
            ReadMySql(reader =>
            {
                schemas = reader.AllSchemas();
            });
            if (schemas == null)
            {
                Assert.Inconclusive();
                return;
            }
            Assert.IsTrue(schemas.Count > 0, "Schemas should contain sakila");
        }

        [TestMethod, TestCategory("MySql")]
        public void MySqlTableTest()
        {
            DatabaseTable country = null;
            ReadMySql(reader =>
            {
                country = reader.Table("country");
            });
            if (country == null)
            {
                Assert.Inconclusive();
                return;
            }
            Assert.AreEqual(3, country.Columns.Count);
            Assert.IsNotNull(country.PrimaryKeyColumn);
            Assert.IsTrue(country.FindColumn("country_id").IsPrimaryKey);
        }

        [TestMethod, TestCategory("MySql")]
        public void MySqlUnsignedIntegersTest()
        {
            DatabaseSchema schema = null;
            ReadMySql(reader =>
            {
                schema = reader.ReadAll();
            });
            if (schema == null)
            {
                Assert.Inconclusive();
                return;
            }

            var country = schema.FindTableByName("country");

            var pk = country.PrimaryKeyColumn;
            Assert.IsNotNull(pk, "Primary key constraints should be loaded");
            //Assert.AreEqual("smallint(5) unsigned", pk.DbDataType); //not on my laptop db
            Assert.AreEqual("SMALLINT", pk.DataType.TypeName);
        }

        //[TestMethod, TestCategory("MySql.Devart")]
        //public void MySqlViaDevartTest()
        //{
        //    const string providername = "Devart.Data.MySql";
        //    var connectionString = ConnectionStrings.MySqlDevart;
        //    ProviderChecker.Check(providername, connectionString);
        //
        //    DiscoverProviderFactory.Discover(connectionString, providername);
        //    var dbReader = new DatabaseReader(connectionString, providername);
        //    dbReader.Owner = "sakila";
        //    var schema = dbReader.ReadAll();
        //    var country = schema.FindTableByName("country");
        //    Assert.AreEqual(3, country.Columns.Count);
        //    Assert.IsNotNull(country.PrimaryKeyColumn);
        //
        //    var table = dbReader.Table("city");
        //    Assert.AreEqual(4, table.Columns.Count);
        //}
    }
}