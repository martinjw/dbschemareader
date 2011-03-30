using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace CopyToSQLite
{
    class Runner
    {
        private readonly DatabaseReader _databaseReader;
        private readonly string _filePath;
        private readonly SqlType _originType;
        private DbProviderFactory _dbFactory;
        private string _originConnection;

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        public void InvokeProgressChanged(int progress, string currentTable)
        {
            var handler = ProgressChanged;
            if (handler != null)
            {
                var e = new ProgressChangedEventArgs(progress, currentTable);
                handler(this, e);
            }
        }

        public Runner(DatabaseReader databaseReader, string filePath, SqlType originType)
        {
            _originType = originType;
            _filePath = filePath;
            _databaseReader = databaseReader;
        }

        public bool Execute()
        {
            InvokeProgressChanged(0, "Reading origin database");
            var databaseSchema = _databaseReader.ReadAll();
            _dbFactory = DbProviderFactories.GetFactory(databaseSchema.Provider);
            _originConnection = databaseSchema.ConnectionString;

            var factory = new DdlGeneratorFactory(SqlType.SqLite);
            var tableGenerator = factory.AllTablesGenerator(databaseSchema);
            tableGenerator.IncludeSchema = false;
            var ddl = tableGenerator.Write();

            var dbCreator = new DatabaseCreator(_filePath);
            InvokeProgressChanged(0, "Creating SQLite database tables");
            dbCreator.CreateTables(ddl);

            //we could work out a proper build order here
            databaseSchema.Tables.Sort((a, b) =>
                                            {
                                                if (a == b) return 0; //the same
                                                if (a == null) return -1; //b is greater
                                                if (b == null) return 1; //a is greater

                                                //b depends on a so a is first
                                                if (b.ForeignKeyChildren.Contains(a)) return -1;
                                                return 1;
                                            });
            var count = databaseSchema.Tables.Count;
            decimal current = 0;
            foreach (var databaseTable in databaseSchema.Tables)
            {
                var percentage = (int)((current / count) * 100);
                InvokeProgressChanged(percentage, "Copying table " + databaseTable.Name);
                if (!Copy(dbCreator, databaseTable)) return false;
                current++;
            }

            return true;
        }

        private bool Copy(DatabaseCreator dbCreator, DatabaseTable databaseTable)
        {
            var originSql = new SqlWriter(databaseTable, _originType);
            var destinationSql = new SqlWriter(databaseTable, SqlType.SqLite);

            var selectAll = originSql.SelectAllSql();
            var insert = destinationSql.InsertSql(true);

            using (var inserter = new DatabaseInserter(dbCreator.CreateConnection(), insert))
            {
                using (var con = _dbFactory.CreateConnection())
                {
                    con.ConnectionString = _originConnection;
                    using (var cmd = con.CreateCommand())
                    {
                        cmd.CommandText = selectAll;
                        con.Open();
                        using (var rdr = cmd.ExecuteReader())
                        {
                            if (rdr.HasRows)
                            {
                                int i = 0;
                                while (rdr.Read())
                                {
                                    i++;
                                    //we only do the first 1000 rows. This is for small databases only!
                                    if (i > 1000) return true;
                                    var result = CopyRow(databaseTable, destinationSql, rdr, inserter);
                                    //if there's a problem, stop doing anything
                                    if (!result) return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static bool CopyRow(DatabaseTable databaseTable, SqlWriter destinationSql, DbDataReader rdr, DatabaseInserter inserter)
        {
            var dictionary = new Dictionary<string, object>();
            if (rdr.FieldCount != databaseTable.Columns.Count)
            {
                //something has gone wrong
                return false;
            }
            for (int index = 0; index < databaseTable.Columns.Count; index++)
            {
                var column = databaseTable.Columns[index];
                var parameterName = destinationSql.ParameterName(column.Name);
                var value = rdr.GetValue(index);
                //we don't care about DBNull here
                dictionary.Add(parameterName, value);
            }
            return inserter.Insert(dictionary);
        }
    }
}
