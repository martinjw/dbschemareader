using System.Linq;
using DatabaseSchemaReader.Extenders.SqlServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class ExtendedSqlServerReader
    {
        [TestMethod]
        public void TestReading()
        {
            const string providername = "System.Data.SqlClient";
            var connectionString = ConnectionStrings.Northwind;
            ProviderChecker.Check(providername, connectionString);

            var dr = new SqlServerDatabaseReader(connectionString);
            dr.Owner = "dbo";
            var schema = dr.ReadAll();

            Assert.IsInstanceOfType(schema, typeof(SqlServerSchema));
            //we have to cast it
            var sqlSchema = (SqlServerSchema) schema;
            //we exposed a typed property for tables.
            //All tables should be SqlServerTables (override SchemaFactory)
            Assert.AreEqual(schema.Tables.Count, sqlSchema.SqlServerTables.Count());
            //pick the first table with stats
            var table = sqlSchema.SqlServerTables.FirstOrDefault(t=> t.DatabaseStatistics.Count > 0);
            Assert.IsNotNull(table); //should be some stats somewhere
        }
    }
}
