using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class MicrosoftSqlClient
    {
        [TestMethod, TestCategory("SqlServer")]
        public void ReadNorthwindSchema()
        {
            var connectionString = ConnectionStrings.Northwind;
            DatabaseSchema schema;
            //using the newer Microsoft Sql client
            using (var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString))
            {
                try
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                }
                catch (Exception exception)
                {
                    Assert.Inconclusive("Cannot access database for provider Microsoft.Data.SqlClient message= " +
                                        exception.Message);
                }

                var dbReader = new DatabaseReader(connection);
                dbReader.Owner = "dbo";
                dbReader.ReadAll();
                schema = dbReader.DatabaseSchema;
            }

            //password is removed in SqlServer 2017
            //Assert.AreEqual(ConnectionStrings.Northwind, schema.ConnectionString, "Connection string is in the schema");
            Assert.IsNotNull(schema.ConnectionString, "Connection string is in the schema");
            Assert.AreEqual("dbo", schema.Owner, "Schema/owner is in the schema");
        }
    }
}
