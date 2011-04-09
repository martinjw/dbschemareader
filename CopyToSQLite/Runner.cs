using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using DatabaseSchemaReader;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;
using DatabaseSchemaReader.Utilities;

namespace CopyToSQLite
{
    /// <summary>
    /// Does the actual work
    /// </summary>
    class Runner
    {
        private readonly DatabaseReader _databaseReader;
        private readonly string _filePath;
        private readonly SqlType _originType;
        private DbProviderFactory _dbFactory;
        private string _originConnection;
        private readonly int _maxiumumRecords;
        private readonly bool _useSqlServerCe;

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

        public Runner(DatabaseReader databaseReader, string filePath, SqlType originType, bool useSqlServerCe)
        {
            _useSqlServerCe = useSqlServerCe;
            _originType = originType;
            _filePath = filePath;
            _databaseReader = databaseReader;
            _maxiumumRecords = Properties.Settings.Default.MaximumRecords;
        }

        public bool Execute()
        {
            InvokeProgressChanged(0, "Reading origin database");
            var databaseSchema = _databaseReader.ReadAll();
            _dbFactory = DbProviderFactories.GetFactory(databaseSchema.Provider);
            _originConnection = databaseSchema.ConnectionString;

            //for SQLServer CE we are actually using the SqlServer settings
            //CE only supports a subset of SqlServer datatypes and functionality
            //the big problem is VARCHAR(MAX) - CE only has NTEXT.
            var factory = new DdlGeneratorFactory(_useSqlServerCe ? SqlType.SqlServer : SqlType.SQLite);
            var tableGenerator = factory.AllTablesGenerator(databaseSchema);
            tableGenerator.IncludeSchema = false;
            var ddl = tableGenerator.Write();

            IDatabaseCreator dbCreator;
            if (_useSqlServerCe)
            {
                dbCreator = new SqlServerCeDatabaseCreator(_filePath);
            }
            else
            {
                dbCreator = new DatabaseCreator(_filePath);
            }
            InvokeProgressChanged(0, "Creating database tables");
            dbCreator.CreateTables(ddl);

            //put them in fk dependency order
            SchemaTablesSorter.Sort(databaseSchema);

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

        private bool Copy(IDatabaseCreator dbCreator, DatabaseTable databaseTable)
        {
            var originSql = new SqlWriter(databaseTable, _originType);
            var destinationSql = new SqlWriter(databaseTable, _useSqlServerCe ? SqlType.SqlServer : SqlType.SQLite);

            var selectAll = originSql.SelectAllSql();
            string insert;
            //SQLServerCE and SQLite can't deal with output parameters, so we can't use the standard INSERT.
            if (_useSqlServerCe)
            {
                //for sqlserver, we don't want an output parameter.
                //there may be FK errors (unless you SET IDENTITY_INSERT myTable ON)
                insert = destinationSql.InsertSqlWithoutOutputParameter();
            }
            else
            {
                //Fortunately SQLite allows you to insert "identity" primary keys
                insert = destinationSql.InsertSqlIncludingIdentity();
            }

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
                                    if (i > _maxiumumRecords) return true;
                                    var result = CopyRow(databaseTable, destinationSql, rdr, inserter);
                                    //if there's a problem, stop doing anything
                                    if (!result)
                                    {
                                        LastErrorMessage = inserter.LastErrorMessage;
                                        return false;
                                    }
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

        public string LastErrorMessage { get; private set; }

    }
}
