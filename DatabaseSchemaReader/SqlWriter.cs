using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader
{
    /// <summary>
    /// Writes simple SQL statements
    /// </summary>
    public class SqlWriter
    {
        private readonly DatabaseTable _table;
        private readonly char _parameterPrefix;
        private readonly string _nameEscapeStart;
        private readonly string _nameEscapeEnd;
        private IList<string> _primaryKeys;
        private readonly SqlType _sqlType;
        private bool _inStoredProcedure;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlWriter"/> class.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="sqlType">Type of the SQL.</param>
        public SqlWriter(DatabaseTable table, SqlType sqlType)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            _table = table;
            _sqlType = sqlType;
            _nameEscapeStart = null;
            _nameEscapeEnd = null;
            switch (sqlType)
            {
                case SqlType.MySql:
                    _parameterPrefix = '?'; //or @ but can conflict with variables
                    _nameEscapeStart = "`"; //backtick, not single apos
                    _nameEscapeEnd = "`";
                    break;
                case SqlType.Oracle:
                    _parameterPrefix = ':';
                    _nameEscapeStart = "\"";
                    _nameEscapeEnd = "\"";
                    break;
                case SqlType.PostgreSql:
                    _parameterPrefix = ':';
                    _nameEscapeStart = "\"";
                    _nameEscapeEnd = "\"";
                    break;
                case SqlType.Db2:
                    _parameterPrefix = '@';
                    _nameEscapeStart = "\"";
                    _nameEscapeEnd = "\"";
                    break;
                case SqlType.SQLite:
                    _parameterPrefix = '@'; //can also be $
                    _nameEscapeStart = "\""; //double quote (supports single quote and square brackets for compat)
                    _nameEscapeEnd = "\"";
                    break;
                //case SqlType.SqlServer: and SqlType.SqlServerCe
                default:
                    _parameterPrefix = '@';
                    _nameEscapeStart = "[";
                    _nameEscapeEnd = "]";
                    break;
            }
        }


        /// <summary>
        /// In stored procedures, Oracle and MySql do not use the parameter prefix. Ignored for SqlServer (which requires @).
        /// </summary>
        /// <value><c>true</c> if in stored procedure; otherwise, <c>false</c>.</value>
        public bool InStoredProcedure
        {
            get { return _inStoredProcedure; }
            set
            {
                if (_sqlType == SqlType.SqlServer) return; //always false
                _inStoredProcedure = value;
            }
        }

        #region Formatting
        /// <summary>
        /// Formats a column name as a parameter.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns></returns>
        public string ParameterName(string columnName)
        {
            //override how column parameters are formatted
            if (FormatParameter != null)
                columnName = FormatParameter(columnName);
            //spaces are valid in escaped names, but definitely not in parameters
            if (columnName.Contains(" ")) columnName = columnName.Replace(" ", "");

            if (!InStoredProcedure)
                columnName = _parameterPrefix + columnName;
            return columnName;
        }

        /// <summary>
        /// Gets the escaped name of the table, including schema if present
        /// </summary>
        /// <value>
        /// The name of the escaped table.
        /// </value>
        public string EscapedTableName
        {
            get
            {
                var name = EscapedName(_table.Name);
                if (!string.IsNullOrEmpty(_table.SchemaOwner) && _table.SchemaOwner != "dbo")
                {
                    name = EscapedName(_table.SchemaOwner) + "." + name;
                }
                return name;
            }
        }

        /// <summary>
        /// Gets the escaped name of a column (or other simple schema object)
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public string EscapedColumnName(string name)
        {
            return EscapedName(name);
        }

        private string EscapedName(string name)
        {
            return _nameEscapeStart + name + _nameEscapeEnd;
        }

        private string FormattedColumns(string[] cols)
        {
            string joinString = _nameEscapeEnd + "," + Environment.NewLine + "  " + _nameEscapeStart;

            string sql = "  " + _nameEscapeStart
                    + String.Join(joinString, cols)
                    + _nameEscapeEnd;
            return sql;
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Gets the columns except identity and timestamps (ie for Insert)
        /// </summary>
        /// <returns></returns>
        private string[] GetColumns()
        {
            var list = new List<string>();
            foreach (var column in _table.Columns)
            {
                //also not SqlServer timestamp
                if (!column.IsIdentity && !column.IsTimestamp() && !column.IsComputed)
                    list.Add(column.Name);
            }
            return list.ToArray();
        }

        private string[] GetAllNoncomputedColumns()
        {
            return _table.Columns
                .Where(c => !c.IsComputed)
                .Select(x => x.Name)
                .ToArray();
        }

        private string[] GetAllColumns()
        {
            var cols = new string[_table.Columns.Count];
            for (int i = 0; i < _table.Columns.Count; i++)
            {
                cols[i] = _table.Columns[i].Name;
            }
            return cols;
        }

        private string AddWhere()
        {
            string where = " WHERE ";

            int numPks = PrimaryKeys.Count;
            var list = new List<string>();
            for (int i = 0; i < numPks; i++)
            {
                var pkName = PrimaryKeys[i];
                list.Add(EscapedName(pkName) + " = " + ParameterName(pkName));
            }
            where += String.Join(" AND ", list.ToArray());
            return where;
        }

        private string AddWhereWithConcurrency()
        {
            string where = AddWhere();
            //there can be only one timestamp/ rowversion per table
            var column = _table.Columns.FirstOrDefault(col => col.IsTimestamp());
            if (column != null)
            {
                var timeStamp = column.Name;
                where +=
                    " AND " +
                    EscapedName(timeStamp) +
                    " = " +
                    ParameterName(timeStamp);
            }
            return where;
        }

        private string PrimaryKeyList()
        {
            //the primary keys as orderBy statements
            var pk = PrimaryKeys;
            int numPks = pk.Count;
            var pks = new string[numPks];
            for (int i = 0; i < numPks; i++)
            {
                var pkName = pk[i];
                pks[i] = EscapedName(pkName);
            }
            return String.Join(", ", pks);
        }
        #endregion

        /// <summary>
        /// Simplify the format- no line breaks, collapse spaces.
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public static string SimpleFormat(string sql)
        {
            sql = Regex.Replace(sql, @"\s{2,}", " ");
            return sql.Trim().Replace(Environment.NewLine, "").Replace("( ", "(").Replace(" )", ")");
        }

        /// <summary>
        /// Optionally override how column parameters are formatted
        /// </summary>
        /// <value>The format parameter function.</value>
        public Func<string, string> FormatParameter { get; set; }

        /// <summary>
        /// Gets the primary keys.
        /// </summary>
        public IList<string> PrimaryKeys
        {
            get
            {
                if (_primaryKeys != null) return _primaryKeys;
                //look up the pk constraint
                if (_table.PrimaryKey != null && _table.PrimaryKey.Columns.Count > 0)
                {
                    _primaryKeys = _table.PrimaryKey.Columns;
                }
                else
                {
                    //no pk constraint, assume first column
                    var result = new List<string>();
                    result.Add(_table.Columns[0].Name);
                    _primaryKeys = result;
                }
                return _primaryKeys;
            }
        }
        /// <summary>
        /// Gets the non primary key columns.
        /// </summary>
        public IList<string> NonPrimaryKeyColumns
        {
            get
            {
                var cols = new List<string>();
                foreach (DatabaseColumn column in _table.Columns)
                {
                    string name = column.Name;
                    //if column is a primary key, we don't update it
                    if (PrimaryKeys.Contains(name)) continue;
                    //also not SqlServer timestamp
                    if (column.IsTimestamp()) continue;
                    //not computer
                    if (column.IsComputed) continue;
                    cols.Add(name);
                }
                return cols;
            }
        }

        /// <summary>
        /// SQL for select by primary key.
        /// </summary>
        /// <returns></returns>
        public string SelectByIdSql()
        {
            return SelectAllSql() +
                Environment.NewLine +
                AddWhere();
        }

        /// <summary>
        /// SQL for select all.
        /// </summary>
        /// <returns></returns>
        public string SelectAllSql()
        {
            var sb = new StringBuilder();
            string[] cols = GetAllColumns();

            sb.AppendLine("SELECT");
            sb.AppendLine(FormattedColumns(cols));
            sb.Append(" FROM " + EscapedTableName);

            return sb.ToString();
        }

        /// <summary>
        /// Paged select. Requires input params: currentPage (1-based), pageSize.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// MySql important: add Allow User Variables=True to connection string
        /// <para>SqlServerCe: use input params offset and pageSize</para>
        /// </remarks>
        public string SelectPageSql()
        {
            var sb = new StringBuilder();
            string[] cols = GetAllColumns();

            string columns = FormattedColumns(cols);
            string orderBy = PrimaryKeyList();

            if (_sqlType == SqlType.MySql)
            {
                sb.AppendLine("set @rownum:=0;");
            }
            sb.AppendLine("SELECT");
            sb.AppendLine(columns);
            sb.AppendLine(" FROM");
            if (_sqlType == SqlType.MySql)
            {
                //outside storedprocedures, consider the LIMIT offset,pageSize syntax
                sb.AppendLine(" (SELECT");
                sb.AppendLine("  (@rownum:= @rownum+1) as rowNumber,");
                sb.AppendLine(columns);
                sb.AppendLine("  FROM " + EscapedTableName);
                sb.AppendLine("  ORDER BY  " + orderBy + ")");
                sb.Append(" AS countedTable");
                sb.AppendLine(" WHERE");
                sb.AppendLine("   rowNumber > (" + ParameterName("pageSize") + " * (" + ParameterName("currentPage") + " - 1))");
                sb.AppendLine("   AND rowNumber <= (" + ParameterName("pageSize") + " * " + ParameterName("currentPage") + ")");
            }
            else if (_sqlType == SqlType.PostgreSql)
            {
                sb.AppendLine(EscapedTableName);
                sb.AppendLine("  ORDER BY  " + orderBy);
                //[LIMIT { number | ALL }] [OFFSET number]
                //NB: we use 1-based page numbers, so add a -1 here!
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "LIMIT {0} OFFSET (({1}-1) * {2})",
                    ParameterName("pageSize"),
                    ParameterName("currentPage"),
                    ParameterName("pageSize")));
            }
            else if (_sqlType == SqlType.SQLite)
            {
                sb.AppendLine(EscapedTableName);
                //LIMIT [offset rows,] <number of rows>
                //NB: we use 1-based page numbers, so add a -1 here!
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "LIMIT (({0}-1) * {1}) ,{2}",
                    ParameterName("currentPage"),
                    ParameterName("pageSize"),
                    ParameterName("pageSize")));
            }
            // //SQLServer 2011 (and SqlServer CE 4.0) syntax.
            else if (_sqlType == SqlType.SqlServerCe)
            {
                WritePagingForSqlServerCe(sb, orderBy);
                //sb.AppendLine(EscapedTableName);
                //sb.AppendLine("  ORDER BY  " + orderBy);
                //sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                //    "OFFSET (({0}-1) * {1}) ROWS FETCH NEXT {2} ROWS ONLY",
                //    ParameterName("currentPage"),
                //    ParameterName("pageSize"),
                //    ParameterName("pageSize")));
            }
            else
            {
                //SQLServer 2005+, Oracle 8+, Db2
                sb.AppendLine("  (SELECT ROW_NUMBER() OVER(ORDER BY " + orderBy + ") AS rowNumber,");
                sb.AppendLine(columns);
                sb.Append("  FROM " + EscapedTableName + ")");
                //SqlServer needs a subquery alias, Oracle doesn't accept it
                if (_sqlType != SqlType.Oracle) sb.Append(" AS countedTable");
                sb.AppendLine(" WHERE");
                sb.AppendLine("   rowNumber > (" + ParameterName("pageSize") + " * (" + ParameterName("currentPage") + " - 1))");
                sb.AppendLine("   AND rowNumber <= (" + ParameterName("pageSize") + " * " + ParameterName("currentPage") + ")");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Paged select. Requires input params: startRow (1 based), endRow.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// MySql important: add Allow User Variables=True to connection string
        /// <para>SqlServerCe: use input params offset and pageSize</para>
        /// </remarks>
        public string SelectPageStartToEndRowSql()
        {
            var sb = new StringBuilder();
            string[] cols = GetAllColumns();

            string columns = FormattedColumns(cols);
            string orderBy = PrimaryKeyList();

            if (_sqlType == SqlType.MySql)
            {
                sb.AppendLine("set @rownum:=0;");
            }
            sb.AppendLine("SELECT");
            sb.AppendLine(columns);
            sb.AppendLine(" FROM");
            if (_sqlType == SqlType.MySql)
            {
                //outside storedprocedures, consider the LIMIT offset,pageSize syntax
                sb.AppendLine(" (SELECT ");
                sb.AppendLine("  (@rownum:=@rownum+1) as rowNumber,");
                sb.AppendLine(columns);
                sb.AppendLine("  FROM " + EscapedTableName);
                sb.AppendLine("  ORDER BY  " + orderBy + ")");
                sb.Append(" AS countedTable");
                sb.AppendLine(" WHERE");
                sb.AppendLine("   rowNumber >= " + ParameterName("startRow"));
                sb.AppendLine("   AND rowNumber <= " + ParameterName("endRow"));
            }
            else if (_sqlType == SqlType.PostgreSql)
            {
                sb.AppendLine(EscapedTableName);
                sb.AppendLine("  ORDER BY  " + orderBy);
                //[LIMIT { number | ALL }] [OFFSET number]
                //NB: we use 1-based page numbers, so add a -1 here!
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "LIMIT ({0} - {1} + 1) OFFSET ({2}-1)",
                    ParameterName("endRow"),
                    ParameterName("startRow"),
                    ParameterName("startRow")));
            }
            else if (_sqlType == SqlType.SQLite)
            {
                sb.AppendLine(EscapedTableName);
                //LIMIT <number of rows> OFFSET <offset rows>
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                    "LIMIT ({0} - {1} + 1) OFFSET ({2}-1)",
                    ParameterName("endRow"),
                    ParameterName("startRow"),
                    ParameterName("startRow")));
            }
            // //SQLServer 2011 (and SqlServer CE 4.0) syntax.
            else if (_sqlType == SqlType.SqlServerCe)
            {
                WritePagingForSqlServerCe(sb, orderBy);
                //sb.AppendLine(EscapedTableName);
                //sb.AppendLine("  ORDER BY  " + orderBy);
                //sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                //                            "   OFFSET {0} - 1 ROWS",
                //                            ParameterName("startRow")));
                //sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                //                            "   FETCH NEXT {0} - {1} + 1 ROWS ONLY",
                //                            ParameterName("endRow"),
                //                            ParameterName("startRow")));
            }
            else
            {
                //SQLServer 2005+, Oracle 8+
                sb.AppendLine("  (SELECT ROW_NUMBER() OVER(ORDER BY " + orderBy + ") AS rowNumber,");
                sb.AppendLine(columns);
                sb.Append("  FROM " + EscapedTableName + ")");
                //SqlServer needs a subquery alias, Oracle doesn't accept it
                if (_sqlType != SqlType.Oracle) sb.Append(" AS countedTable");
                sb.AppendLine(" WHERE");
                sb.AppendLine("   rowNumber >= " + ParameterName("startRow"));
                sb.AppendLine("   AND rowNumber <= " + ParameterName("endRow"));
            }

            return sb.ToString();
        }

        private void WritePagingForSqlServerCe(StringBuilder sb, string orderBy)
        {
            //Sql Server 2011 will have the ORDER BY x OFFSET n ROWS FETCH NEXT n ROWS ONLY syntax (FETCH now is cursors only) 
            //denali msdn is http://msdn.microsoft.com/en-us/library/ms188385%28v=sql.110%29.aspx#Offset
            /*
SELECT DepartmentID, Name, GroupName
FROM HumanResources.Department
ORDER BY DepartmentID ASC 
OFFSET @StartingRowNumber - 1 ROWS 
FETCH NEXT @EndingRowNumber - @StartingRowNumber + 1 ROWS ONLY
             */
            //SqlServerCE 4 has this syntax, but it seems more limited 
            //(apparently you can't use expressions as described in msdn for denali)
            //ce msdn is http://msdn.microsoft.com/en-us/library/gg699618%28v=SQL.110%29.aspx
            //
            sb.AppendLine(EscapedTableName);
            sb.AppendLine("  ORDER BY  " + orderBy);
            //OFFSET @StartingRowNumber - 1 ROWS 
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                        "   OFFSET {0} ROWS",
                                        ParameterName("offset")));
            //FETCH NEXT @EndingRowNumber - @StartingRowNumber + 1 ROWS ONLY
            sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                                        "   FETCH NEXT {0} ROWS ONLY",
                                        ParameterName("pageSize")));
        }

        /// <summary>
        /// SQL for select with where clause for specified column.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public string SelectWhereSql(string column)
        {
            return SelectAllSql() +
                Environment.NewLine +
                " WHERE " +
                Environment.NewLine +
                "  " + EscapedName(column) + " = " + ParameterName(column);
        }

        /// <summary>
        /// SQL for count all.
        /// </summary>
        /// <returns></returns>
        public string CountSql()
        {
            return "SELECT COUNT(*) FROM " + EscapedTableName;
        }

        /// <summary>
        /// SQL for count all with output parameter.
        /// </summary>
        /// <param name="outputParameter">The output parameter.</param>
        /// <returns></returns>
        public string CountSql(string outputParameter)
        {
            string what = outputParameter + " = COUNT(*)";
            if (_sqlType == SqlType.Oracle || _sqlType == SqlType.Db2)
                what = "COUNT(*) INTO " + outputParameter;
            return "SELECT " + what + " FROM " + EscapedTableName;
        }

        /// <summary>
        /// SQL for delete by primary key.
        /// </summary>
        /// <returns></returns>
        public string DeleteSql()
        {
            var sb = new StringBuilder();
            sb.AppendLine("DELETE FROM " + EscapedTableName);
            sb.Append(AddWhere());

            return sb.ToString();
        }

        /// <summary>
        /// SQL for insert new row.
        /// </summary>
        /// <returns></returns>
        public string InsertSql()
        {
            return InsertSql(true, false);
        }

        /// <summary>
        /// SQL for insert new row including any identity column.
        /// </summary>
        /// <returns></returns>
        public string InsertSqlIncludingIdentity()
        {
            return InsertSql(false, true);
        }

        /// <summary>
        /// SQL for insert new row without output parameter
        /// </summary>
        /// <returns></returns>
        public string InsertSqlWithoutOutputParameter()
        {
            return InsertSql(false, false);
        }

        private string InsertSql(bool useOutputParameter, bool includeIdentityInInsert)
        {
            var sb = new StringBuilder();
            string[] cols;
            if (!includeIdentityInInsert)
                cols = GetColumns(); //excluding identity and timestamps
            else
                cols = GetAllNoncomputedColumns(); //incl indentity

            var values = new string[cols.Length];
            for (int i = 0; i < cols.Length; i++)
            {
                values[i] = ParameterName(cols[i]);
            }
            string joinString = "," + Environment.NewLine + "  ";

            sb.AppendLine("INSERT INTO " + EscapedTableName + " (");
            sb.AppendLine(FormattedColumns(cols));
            sb.AppendLine(") VALUES (");
            sb.Append(" ");
            sb.AppendLine(String.Join(joinString, values));
            sb.Append(")");
            if (_table.HasIdentityColumn)
            {
                string identityParameter = FindIdentityParameter();
                if (_sqlType == SqlType.Oracle && useOutputParameter)
                {
                    //a primary key assigned from a sequence by a trigger
                    var pk = EscapedName(PrimaryKeys[0]);
                    sb.AppendLine(" RETURNING " + pk + " INTO " + identityParameter + "");
                }
                else if (_sqlType == SqlType.SqlServer)
                {
                    sb.AppendLine(";");
                    if (useOutputParameter)
                    {
                        sb.Append("SET " + identityParameter + " = SCOPE_IDENTITY();");
                    }
                    else
                    {
                        sb.Append("SELECT  SCOPE_IDENTITY();");
                    }
                }
                else if (_sqlType == SqlType.MySql)
                {
                    sb.AppendLine(";");
                    if (useOutputParameter)
                    {
                        //sb.Append("SELECT LAST_INSERT_ID() INTO " + identityParameter + ";");
                        sb.Append("SET " + identityParameter + " = LAST_INSERT_ID();");
                    }
                    else
                    {
                        sb.Append("SELECT LAST_INSERT_ID();");
                    }
                }
                else if (_sqlType == SqlType.PostgreSql)
                {
                    sb.AppendLine(";");
                    //default sequence name is tablename_colname_seq
                    var seq = _table.Name + "_" + ((_table.PrimaryKeyColumn != null) ? _table.PrimaryKeyColumn.Name : null) + "_seq";
                    sb.Append("SELECT currval('" + seq + "');");
                }
                else if (_sqlType == SqlType.SQLite)
                {
                    //SQLite doesn't have output parameters
                    sb.AppendLine(";");
                    sb.Append("SELECT last_insert_rowid();");
                }
                else if (_sqlType == SqlType.Db2)
                {
                    sb.AppendLine(";");
                    var identity = FindIdentityColumn();
                    if (useOutputParameter)
                    {
                        //may need to cast this to from decimal(13,0)
                        if (identity.DbDataType.ToUpperInvariant() == "INTEGER")
                            sb.AppendLine("VALUES INTEGER(IDENTITY_VAL_LOCAL()) INTO " + identityParameter + ";");
                        else
                            sb.AppendLine("VALUES IDENTITY_VAL_LOCAL() INTO " + identityParameter + ";");
                    }
                    else
                    {
                        sb.AppendLine("SELECT IDENTITY_VAL_LOCAL() FROM SYSIBM.SYSDUMMY1;");
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Finds the identity parameter. May be null if no identity.
        /// </summary>
        /// <returns></returns>
        private string FindIdentityParameter()
        {
            DatabaseColumn identityColumn = FindIdentityColumn();
            if (identityColumn == null) return null;
            string identityParameter = ParameterName(identityColumn.Name);
            return identityParameter;
        }

        private DatabaseColumn FindIdentityColumn()
        {
            return _table.Columns.Find(delegate(DatabaseColumn col)
                    {
                        return col.IsIdentity;
                    });
        }

        /// <summary>
        /// SQL for update row.
        /// </summary>
        /// <returns></returns>
        public string UpdateSql()
        {
            var sb = new StringBuilder();

            var cols = new List<string>();
            foreach (string name in NonPrimaryKeyColumns)
            {
                cols.Add(EscapedName(name) + " = " + ParameterName(name));
            }
            //no primary keys. Just select and ignore.
            if (cols.Count == 0) return "SELECT 1";

            sb.AppendLine("UPDATE " + EscapedTableName + " SET ");
            sb.AppendLine(String.Join("," + Environment.NewLine + " ", cols.ToArray()));
            sb.Append(AddWhereWithConcurrency());

            return sb.ToString();
        }
    }
}
