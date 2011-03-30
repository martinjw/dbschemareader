using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;

namespace CopyToSQLite
{
    class DatabaseCreator
    {
        private readonly string _connectionString;

        public DatabaseCreator(string filePath)
        {
            SQLiteConnection.CreateFile(filePath);
            var csb = new SQLiteConnectionStringBuilder();
            csb.DataSource = filePath;
            _connectionString = csb.ConnectionString;
        }

        public SQLiteConnection CreateConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public void CreateTables(string ddl)
        {
            using (var con = CreateConnection())
            {
                using (var cmd = new SQLiteCommand(ddl, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public bool InsertSingle(string insert, IDictionary<string, object> dictionary)
        {
            //this is slow
            bool result;
            using (var con = CreateConnection())
            {
                using (var cmd = new SQLiteCommand(insert, con))
                {
                    foreach (var column in dictionary)
                    {
                        cmd.Parameters.AddWithValue(column.Key, column.Value);
                    }
                    con.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = true;
                    }
                    catch (SQLiteException exception)
                    {
                        Debug.WriteLine(exception.Message);
                        result = false;
                    }
                }
            }
            return result;
        }
    }
}
