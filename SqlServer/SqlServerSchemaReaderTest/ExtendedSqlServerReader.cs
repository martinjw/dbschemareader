using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlServerSchemaReader;
using SqlServerSchemaReader.Schema;

namespace SqlServerSchemaReaderTest
{
    [TestClass]
    public class ExtendedSqlServerReader
    {
        [TestMethod]
        public void TestReading()
        {
            const string providername = "System.Data.SqlClient";
            var connectionString = ConnectionStrings.TestSchema;
            ProviderChecker.Check(providername, connectionString);

            var dr = new SqlServerDatabaseReader(connectionString) {Owner = "dbo"};
            var schema = dr.ReadAll();

            Assert.IsInstanceOfType(schema, typeof(SqlServerSchema));
            //we have to cast it
            var sqlSchema = (SqlServerSchema) schema;
            //we exposed a typed property for tables.
            //All tables should be SqlServerTables (override SchemaFactory)
            var countOfSqlServerTables = sqlSchema.SqlServerTables.Count();
            Assert.AreEqual(schema.Tables.Count, countOfSqlServerTables);
            //give up if no tables
            if (countOfSqlServerTables == 0) return;
            //pick the first table with stats
            var table = sqlSchema.SqlServerTables.FirstOrDefault(t=> t.DatabaseStatistics.Count > 0);
            Assert.IsNotNull(table); //should be some stats somewhere
        }
    }
}
