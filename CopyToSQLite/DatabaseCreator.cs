using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;

namespace CopyToSQLite
{
    class DatabaseCreator : IDatabaseCreator
    {
        private readonly string _connectionString;

        public DatabaseCreator(string filePath)
        {
            SQLiteConnection.CreateFile(filePath);
            var csb = new SQLiteConnectionStringBuilder();
            csb.DataSource = filePath;
            _connectionString = csb.ConnectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        public void CreateTables(string ddl)
        {
            using (var con = CreateConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = ddl;
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
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    cmd.CommandText = insert;

                    foreach (var column in dictionary)
                    {
                        var par = cmd.CreateParameter();
                        par.ParameterName = column.Key;
                        par.Value = column.Value;
                        cmd.Parameters.Add(par);
                    }
                    con.Open();
                    try
                    {
                        cmd.ExecuteNonQuery();
                        result = true;
                    }
                    catch (DbException exception)
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
