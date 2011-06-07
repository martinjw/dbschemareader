using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Data
{
    /// <summary>
    /// Reads data from a table into a DataTable. SELECT statement is generated.
    /// </summary>
    /// <remarks>
    /// Uses the <see cref="SqlWriter"/> to generate the SELECT statement.
    /// </remarks>
    public class Reader
    {
        private readonly DatabaseTable _databaseTable;
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

        private SqlType FindSqlType()
        {
            var sqlType = ProviderToSqlType.Convert(_providerName);
            return !sqlType.HasValue ? SqlType.SqlServer : sqlType.Value;
        }

        /// <summary>
        /// Reads ALL the data from the table into a DataTable. Small tables only.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "We're generating the select SQL")]
        public DataTable Read()
        {

            var originSql = new SqlWriter(_databaseTable, FindSqlType());
            var selectAll = originSql.SelectAllSql();

            var dt = new DataTable(_databaseTable.Name) { Locale = CultureInfo.InvariantCulture };

            var dbFactory = DbProviderFactories.GetFactory(_providerName);
            using (var con = dbFactory.CreateConnection())
            {
                con.ConnectionString = _connectionString;
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = selectAll;
                    using (var da = dbFactory.CreateDataAdapter())
                    {
                        da.SelectCommand = cmd;
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
    }
}
