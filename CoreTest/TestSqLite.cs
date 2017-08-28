using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreTest
{
    [TestClass]
    public class TestSqLite
    {
        [TestMethod]
        public void RunSqlite()
        {
            using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "CREATE TABLE Test(Value);";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "INSERT INTO Test VALUES(1);";
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        "SELECT [type], [name], [tbl_name], [rootpage], [sql], [rowid] FROM [main].[sqlite_master] WHERE [type] LIKE 'table'";
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            System.Console.WriteLine(dr[2].ToString());
                        }
                    }
                }

                var databaseReader = new DatabaseSchemaReader.DatabaseReader(connection);
                var schema = databaseReader.ReadAll();
                var tableList = databaseReader.TableList();
                var tables = databaseReader.AllTables();
                var views = databaseReader.AllViews();
                Assert.IsTrue(tableList.Count > 0);
            }
        }
    }
}