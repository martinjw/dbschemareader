using System.Data.SQLite;
using System.Transactions;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class CompareSQLiteDatabases
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
            var dsr1 = new DatabaseReader("Data Source=" + Db1Sqlite + ";Version=3;", SqlType.SQLite);
            var schema1 = dsr1.ReadAll();
            var dsr2 = new DatabaseReader("Data Source=" + Db2Sqlite + ";Version=3;", SqlType.SQLite);
            var schema2 = dsr2.ReadAll();
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


        private static void CreateDatabases()
        {
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
            SQLiteConnection.CreateFile(fileName);
            using (var tran = new TransactionScope())
            {
                using (var con = new SQLiteConnection("Data Source=" + fileName + ";Version=3;"))
                {
                    con.Open();
                    using (var command = new SQLiteCommand(script, con))
                    {
                        command.ExecuteNonQuery();
                    }
                    tran.Complete();
                }
            }
        }
    }
}
