using System;
using System.Data.Common;
using System.Globalization;
using DatabaseSchemaReader.Utilities;

namespace CopyToSQLite
{
    class SqlServerCeDatabaseCreator : IDatabaseCreator
    {
        //normally you must reference C:\Program Files\Microsoft SQL Server Compact Edition\v3.5\Desktop
        // (it's not in the GAC)
        //here, we just use reflection and dynamic magic to avoid the hard reference
        //then it works even if SqlServer CE 4 isn't on the machine

        private readonly string _connectionString;

        public SqlServerCeDatabaseCreator(string filePath)
        {
            //if you want a password, add it with single quotes
            _connectionString = string.Format(CultureInfo.InvariantCulture, "DataSource=\"{0}\";", filePath);

            var factory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");

            //no direct reference, so a little reflection will create the database file
            var asm = factory.GetType().Assembly;
            var engineType = asm.GetType("System.Data.SqlServerCe.SqlCeEngine",false, true);
            if (engineType == null)
            {
                //couldn't find reference
                return;
            }

            using (dynamic engine = Activator.CreateInstance(engineType))
            {
                engine.LocalConnectionString = _connectionString;
                engine.CreateDatabase();
            }
        }
        #region Implementation of IDatabaseCreator

        public DbConnection CreateConnection()
        {
            var factory = DbProviderFactories.GetFactory("System.Data.SqlServerCe.4.0");
            var dbConnection = factory.CreateConnection();
            dbConnection.ConnectionString = _connectionString;
            return dbConnection;
        }

        public void CreateTables(string ddl)
        {
            var statements = ScriptTools.SplitScript(ddl);

            using (var con = CreateConnection())
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.Connection = con;
                    con.Open();
                    //break dll into separate statements and execute them.
                    foreach (var batch in statements)
                    {
                        foreach (var statement in ScriptTools.SplitBySemicolon(batch))
                        {
                            //ignore the drop table bit, which has no useful commands
                            if (statement.StartsWith("-- DROP TABLE", StringComparison.OrdinalIgnoreCase)) continue;
                            if (statement.StartsWith("-- ALTER TABLE", StringComparison.OrdinalIgnoreCase)) continue;
                            cmd.CommandText = statement;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
