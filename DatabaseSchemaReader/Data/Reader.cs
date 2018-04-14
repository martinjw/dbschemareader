﻿using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Data
{
    /// <summary>
    /// Reads data from a table into a DataTable. SELECT statement is generated.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="SqlWriter" /> to generate the SELECT statement.
    /// </remarks>
    public class Reader
    {
        private readonly DatabaseTable _databaseTable;
        private int _pageSize = 1000;
        private readonly string _connectionString;
        private readonly string _providerName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">Name of the provider.</param>
        public Reader(DatabaseTable databaseTable, string connectionString, string providerName)
        {
            if (databaseTable == null)
                throw new ArgumentNullException("databaseTable");
            if (connectionString == null)
                throw new ArgumentNullException("connectionString");
            if (providerName == null)
                throw new ArgumentNullException("providerName");
            _providerName = providerName;
            _connectionString = connectionString;
            _databaseTable = databaseTable;
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        public Reader(DatabaseTable databaseTable)
        {
            //This constructor is only used in NetStandard.
            //The Read(connection) methods would be ok, but if the non-connection methods will not have a connection string...
            if (databaseTable == null)
                throw new ArgumentNullException("databaseTable");
            _databaseTable = databaseTable;
        }

#else
        /// <summary>
        /// Reads first x rows of data from the table into a DataTable.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
                "CA2100:Review SQL queries for security vulnerabilities", Justification = "We're generating the select SQL")
        ]
        public DataTable Read()
        {
            var sqlType = FindSqlType(_providerName);
            var originSql = new SqlWriter(_databaseTable, sqlType);
            var selectAll = originSql.SelectPageSql();

            var dt = new DataTable(_databaseTable.Name) { Locale = CultureInfo.InvariantCulture };

            var dbFactory = DbProviderFactories.GetFactory(_providerName);
            using (var con = dbFactory.CreateConnection())
            {
                con.ConnectionString = _connectionString;
                using (var cmd = con.CreateCommand())
                {
                    BuildCommand(cmd, selectAll, sqlType);

                    using (var da = dbFactory.CreateDataAdapter())
                    {
                        da.SelectCommand = cmd;
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
#endif
        /// <summary>
        /// Reads first x rows of data from the table into a DataTable.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2100:Review SQL queries for security vulnerabilities", Justification = "We're generating the select SQL")
        ]
        public DataTable Read(DbConnection connection)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (string.IsNullOrEmpty(connection.ConnectionString))
                throw new ArgumentException("Connection has no connection string");

            var providerName = _providerName;
            if (string.IsNullOrEmpty(providerName)) providerName = connection.GetType().Namespace;
            var sqlType = FindSqlType(providerName);
#if NETSTANDARD2_0
            var dbFactory = FactoryFinder.FindFactory(connection);
#else
            var dbFactory = DbProviderFactories.GetFactory(providerName);
#endif
            var originSql = new SqlWriter(_databaseTable, sqlType);
            var selectAll = originSql.SelectPageSql();

            var dt = new DataTable(_databaseTable.Name) { Locale = CultureInfo.InvariantCulture };
            if (dbFactory == null)
            {
                Console.WriteLine("Cannot find the DbProviderFactory (provider not netstandard 2.0?)");
                return dt;
            }
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                BuildCommand(cmd, selectAll, sqlType);

                using (var da = dbFactory.CreateDataAdapter())
                {
                    da.SelectCommand = cmd;
                    da.Fill(dt);
                }
            }
            return dt;
        }

        private void BuildCommand(DbCommand cmd, string selectAll, SqlType sqlType)
        {
            cmd.CommandText = selectAll;
            var p = cmd.CreateParameter();
            var parameterName = "currentPage";
            if (sqlType == SqlType.SqlServerCe) parameterName = "offset";
            p.ParameterName = parameterName;
            p.Value = 1;
            if (sqlType == SqlType.SqlServerCe) p.Value = 0;
            cmd.Parameters.Add(p);
            var ps = cmd.CreateParameter();
            ps.ParameterName = "pageSize";
            ps.Value = _pageSize;
            cmd.Parameters.Add(ps);
        }

        /// <summary>
        /// Reads data from the table and invokes a function. if the function returns FALSE execution stops. PageSize is not honored.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2100:Review SQL queries for security vulnerabilities", Justification =
                "We're generating the select SQL")]
        public void Read(DbConnection connection, Func<IDataRecord, bool> processRecord)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (processRecord == null) return;
            if (string.IsNullOrEmpty(connection.ConnectionString))
                throw new ArgumentException("Connection has no connection string");

            var providerName = _providerName;
            if (string.IsNullOrEmpty(providerName)) providerName = connection.GetType().Namespace;
            var sqlType = FindSqlType(providerName);

            var originSql = new SqlWriter(_databaseTable, sqlType);
            var sql = originSql.SelectAllSql();

            RunCommand(connection, processRecord, sql);
        }

        private static void RunCommand(DbConnection connection, Func<IDataRecord, bool> processRecord, string sql)
        {
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = sql;
                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.HasRows) return;
                    while (dr.Read())
                    {
                        if (!processRecord(dr)) return;
                    }
                }
            }
        }
#if !NETSTANDARD2_0

        /// <summary>
        /// Reads data from the table and invokes a function. if the function returns FALSE execution stops. PageSize is not honored.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
            "CA2100:Review SQL queries for security vulnerabilities", Justification = "We're generating the select SQL")]
        public void Read(Func<IDataRecord, bool> processRecord)
        {
            if (processRecord == null) return;
            var sqlType = FindSqlType(_providerName);
            var originSql = new SqlWriter(_databaseTable, sqlType);
            var sql = originSql.SelectAllSql();

            var dbFactory = DbProviderFactories.GetFactory(_providerName);
            using (var connection = dbFactory.CreateConnection())
            {
                connection.ConnectionString = _connectionString;
                RunCommand(connection, processRecord, sql);
            }
        }
#endif

        private SqlType FindSqlType(string providerName)
        {
            var sqlType = ProviderToSqlType.Convert(providerName);
            return !sqlType.HasValue ? SqlType.SqlServer : sqlType.Value;
        }

        /// <summary>
        /// Gets or sets the maximum number of records returned. Default is 1000, maximum is 10000.
        /// </summary>
        /// <value>The size of the page.</value>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (value <= 0) throw new InvalidOperationException("Must be a positive number");
                if (value > 10000) throw new InvalidOperationException("Value is too large - consider another method");
                _pageSize = value;
            }
        }
    }
}
