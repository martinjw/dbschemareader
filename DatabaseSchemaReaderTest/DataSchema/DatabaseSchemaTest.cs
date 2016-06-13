using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.DataSchema
{
    [TestClass]
    public class DatabaseSchemaTest
    {
        [TestMethod]
        public void TestSqlTypeConstructor()
        {
            var schema = new DatabaseSchema(null, SqlType.SqlServer);
            //translated to correct provider name
            Assert.AreEqual("System.Data.SqlClient", schema.Provider);
            //it round trips back to SqlType
            Assert.AreEqual(SqlType.SqlServer, ProviderToSqlType.Convert(schema.Provider));

            //we'll check all the others in same test
            schema = new DatabaseSchema(null, SqlType.Oracle);
            Assert.AreEqual("System.Data.OracleClient", schema.Provider);
            Assert.AreEqual(SqlType.Oracle, ProviderToSqlType.Convert(schema.Provider));

            schema = new DatabaseSchema(null, SqlType.MySql);
            Assert.AreEqual("MySql.Data.MySqlClient", schema.Provider);
            Assert.AreEqual(SqlType.MySql, ProviderToSqlType.Convert(schema.Provider));

            schema = new DatabaseSchema(null, SqlType.SQLite);
            Assert.AreEqual("System.Data.SQLite", schema.Provider);
            Assert.AreEqual(SqlType.SQLite, ProviderToSqlType.Convert(schema.Provider));

            schema = new DatabaseSchema(null, SqlType.SqlServerCe);
            Assert.AreEqual("System.Data.SqlServerCe.4.0", schema.Provider);
            Assert.AreEqual(SqlType.SqlServerCe, ProviderToSqlType.Convert(schema.Provider));

            schema = new DatabaseSchema(null, SqlType.PostgreSql);
            Assert.AreEqual("Npgsql", schema.Provider);
            Assert.AreEqual(SqlType.PostgreSql, ProviderToSqlType.Convert(schema.Provider));

            schema = new DatabaseSchema(null, SqlType.Db2);
            Assert.AreEqual("IBM.Data.DB2", schema.Provider);
            Assert.AreEqual(SqlType.Db2, ProviderToSqlType.Convert(schema.Provider));
        }

        [TestMethod]
        public void TestInitializeCollections()
        {
            var schema = new DatabaseSchema(null, null);
            Assert.IsNotNull(schema.Tables);
            Assert.IsNotNull(schema.Views);
            Assert.IsNotNull(schema.Users);
            Assert.IsNotNull(schema.StoredProcedures);
            Assert.IsNotNull(schema.Sequences);
            Assert.IsNotNull(schema.Packages);
            Assert.IsNotNull(schema.Functions);
            Assert.IsNotNull(schema.DataTypes);
        }

        [TestMethod]
        public void TestFindByName()
        {
            var schema = new DatabaseSchema(null, null);
            schema.AddTable("Orders")
                .AddTable("Products");

            var table = schema.FindTableByName("products");
            Assert.IsNotNull(table);
            Assert.AreEqual("Products", table.Name);

        }
    }
}
