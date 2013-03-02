using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace DatabaseSchemaReader.Data
{
    /// <summary>
    /// Writes SQL INSERT statements
    /// </summary>
    public class InsertWriter
    {
        private readonly DatabaseTable _databaseTable;
        private readonly DataTable _dataTable;
        private SqlWriter _sqlWriter;
        private string _template;
        private readonly IDictionary<string, Type> _columnTypes = new Dictionary<string, Type>();
        private readonly IDictionary<string, string> _dateTypes = new Dictionary<string, string>();
        private readonly IList<string> _nullColumns = new List<string>();
        private SqlType _sqlType;
        private Converter _converter;

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertWriter"/> class.
        /// </summary>
        /// <param name="databaseTable">The databaseTable.</param>
        /// <param name="dataTable">The data table.</param>
        public InsertWriter(DatabaseTable databaseTable, DataTable dataTable)
        {
            if (databaseTable == null)
                throw new ArgumentNullException("databaseTable");
            if (dataTable == null)
                throw new ArgumentNullException("dataTable");

            _dataTable = dataTable;
            _databaseTable = databaseTable;

            PrepareTypes();
        }

        private void PrepareTypes()
        {
            foreach (var databaseColumn in _databaseTable.Columns)
            {
                var key = databaseColumn.Name;
                if (!_dataTable.Columns.Contains(key))
                {
                    _columnTypes.Add(key, typeof(object));
                    continue;
                }
                var columnType = _dataTable.Columns[key].DataType;
                _columnTypes.Add(key, columnType);
                if (columnType == typeof(DateTime))
                {
                    //get the original database type (datetime2, date, time, timestamp etc)
                    _dateTypes.Add(key, databaseColumn.DbDataType.ToUpperInvariant());
                }
                if (columnType == typeof(object))
                {
                    _nullColumns.Add(key);
                }
                if (!IncludeBlobs && DataTypeConverter.IsBlob(databaseColumn.DbDataType.ToUpperInvariant(), databaseColumn))
                {
                    _nullColumns.Add(key);
                }

            }
        }

        /// <summary>
        /// Include identity values in INSERTs
        /// </summary>
        /// <value>
        ///   <c>true</c> if include identity; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeIdentity { get; set; }


        /// <summary>
        /// Include BLOB in INSERTS. This is only practical for small blobs for certain databases (eg it works in SqlServer Northwind).
        /// </summary>
        /// <value><c>true</c> if include blobs; otherwise, <c>false</c>.</value>
        public bool IncludeBlobs { get; set; }

        /// <summary>
        /// Writes the INSERTs in the specified SQL dialect
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        /// <returns></returns>
        public string Write(SqlType sqlType)
        {
            _sqlType = sqlType;
            _sqlWriter = new SqlWriter(_databaseTable, sqlType);
            _converter = new Converter(sqlType, _dateTypes);

            PrepareTemplate();

            var sb = new StringBuilder();

            PrepareIdentityInsert(sb);

            foreach (DataRow row in _dataTable.Rows)
            {
                sb.AppendLine(WriteInsert(row));
            }

            ResetIdentity(sb);

            return sb.ToString();
        }

        #region Identity
        private void PrepareIdentityInsert(StringBuilder sb)
        {
            if (!IncludeIdentity || (_sqlType != SqlType.SqlServer && _sqlType != SqlType.SqlServerCe) ||
                !_databaseTable.HasIdentityColumn) return;

            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT {0} ON", _sqlWriter.EscapedTableName));
            sb.AppendLine("GO");
        }

        private void ResetIdentity(StringBuilder sb)
        {
            if (!IncludeIdentity) return;
            if (_sqlType != SqlType.SqlServer && _sqlType != SqlType.SqlServerCe) return;
            if (!_databaseTable.HasIdentityColumn) return;

            var tableName = _sqlWriter.EscapedTableName;
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "SET IDENTITY_INSERT {0} OFF", tableName));
            sb.AppendLine("GO");
            if (_sqlType == SqlType.SqlServer)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "DBCC CHECKIDENT ({0})", tableName));
            }
            else //SqlServer CE
            {
                var identityColumn = _databaseTable.Columns.First(c => c.IsIdentity).Name;
                sb.AppendLine("GO");
                sb.AppendLine("DECLARE @MAX int;");
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                            "SELECT @MAX = MAX([{0}])+1 FROM {1};", identityColumn, tableName));
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                            "ALTER TABLE {0} ALTER COLUMN [{1}] IDENTITY (@MAX,1);", tableName, identityColumn));
            }
            sb.AppendLine("GO");
        }
        #endregion

        /// <summary>
        /// Writes the insert statement for the specified data.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns></returns>
        public string WriteInsert(DataRow row)
        {
            var values = new List<string>(_databaseTable.Columns.Count);

            foreach (var databaseColumn in _databaseTable.Columns)
            {
                if (!IncludeIdentity && databaseColumn.IsIdentity) continue;

                if (_nullColumns.Contains(databaseColumn.Name))
                {
                    values.Add("NULL");
                    continue;
                }

                var columnType = _columnTypes[databaseColumn.Name];

                object data = row[databaseColumn.Name];
                values.Add(_converter.Convert(columnType, data, databaseColumn.Name));
            }

            var value = string.Join(" ,", values.ToArray());
            return string.Format(CultureInfo.InvariantCulture, _template, value);
        }


        private void PrepareTemplate()
        {
            var cols = GetAllColumns();

            _template = "INSERT INTO " + _sqlWriter.EscapedTableName + @" (
" + FormattedColumns(cols) + @") VALUES (
{0}
);
";
        }

        private static string FormattedColumns(string[] cols)
        {
            const string joinString = ",  ";
            string sql = "  " + String.Join(joinString, cols);
            return sql;
        }

        private string[] GetAllColumns()
        {
            var cols = new List<string>();
            foreach (var databaseColumn in _databaseTable.Columns)
            {
                if (!IncludeIdentity && databaseColumn.IsIdentity) continue;
                cols.Add(_sqlWriter.EscapedColumnName(databaseColumn.Name));
            }

            return cols.ToArray();
        }

    }
}
