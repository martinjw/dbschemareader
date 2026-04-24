using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    public class CompareSqLiteDatabases
    {
        //we can't use 2 :memory: databases
        private const string Db1Sqlite = "db1.sqlite";

        private const string Db2Sqlite = "db2.sqlite";

        [TestMethod]
        public void Given2DatabasesThenDifferencesFound()
        {
            //arrange
            CreateDatabases();

            //act
            var schema1 = ReadAll(Db1Sqlite);
            var schema2 = ReadAll(Db2Sqlite);
            var comparison = new CompareSchemas(schema1, schema2);
            var result = comparison.Execute();

            //assert
            //script should look like this...
            //-- ADDED TABLE Products COLUMN Cost
            //ALTER TABLE [Products] ADD [Cost] NUMERIC DEFAULT 0 NOT NULL;
            //-- Products from [Name] TEXT NOT NULL to [Name] TEXT NULL
            //-- TODO: change manually (no ALTER COLUMN)

            Assert.IsTrue(result.Contains("ALTER TABLE [Products] ADD [Cost]"));
            Assert.IsTrue(result.Contains("TO CHANGE COLUMN"));
        }

        private DatabaseSchema ReadAll(string filePath)
        {
            var connectionString = BuildConnectionString(filePath);

            using (var con = new SqliteConnection(connectionString))
            {
                con.Open();
                var dsr1 = new DatabaseReader(con);
                return dsr1.ReadAll();
            }
        }

        private static string BuildConnectionString(string filePath)
        {
            var csb = new SqliteConnectionStringBuilder { DataSource = Path.Combine(Environment.CurrentDirectory, filePath) };
            var connectionString = csb.ConnectionString;
            return connectionString;
        }

        private static void CreateDatabases()
        {
            try
            {
                File.Delete(Path.Combine(Environment.CurrentDirectory, Db1Sqlite));
                File.Delete(Path.Combine(Environment.CurrentDirectory, Db2Sqlite));
            }
            catch (Exception)
            {
                //clean out before we recreate - ignore any errors
            }

            var schema = new DatabaseSchema(null, SqlType.SQLite);
            var products = schema.AddTable("Products");
            products.AddColumn<int>("Id").AddPrimaryKey()
                .AddColumn<string>("Name").AddLength(20);

            var script = CreateDatabaseScript(schema);
            CreateSQLiteDatabase(script, Db1Sqlite);

            //add new column
            products.AddColumn<int>("Cost").AddPrecisionScale(9, 2);
            //change nullable (alter column) - nb length is always 2147483647
            products.FindColumn("Name").AddLength(30).AddNullable();

            script = CreateDatabaseScript(schema);
            CreateSQLiteDatabase(script, Db2Sqlite);
        }

        private static string CreateDatabaseScript(DatabaseSchema schema)
        {
            var ddlFactory = new DdlGeneratorFactory(SqlType.SQLite);
            var generator = ddlFactory.AllTablesGenerator(schema);
            return generator.Write();
        }

        // ReSharper disable once InconsistentNaming
        private static void CreateSQLiteDatabase(string script, string fileName)
        {
            InitSqLite.CreateSqlite(fileName, script);
        }
    }
}