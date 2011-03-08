using System;
using System.IO;
using System.Text;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    ///<summary>
    /// Generate stored procedures (standard CRUD types including paging)
    ///</summary>
    /// <remarks>
    /// Override this with platform specific procedure generators
    /// </remarks>
    internal abstract class ProcedureGeneratorBase : IProcedureGenerator
    {
        protected readonly DatabaseTable Table;
        protected readonly string TableName;
        protected readonly StringBuilder File = new StringBuilder();
        protected SqlWriter SqlWriter;
        private string _path;
        private string _scriptPath;
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureGeneratorBase"/> class.
        /// </summary>
        /// <param name="table">The table.</param>
        protected ProcedureGeneratorBase(DatabaseTable table)
        {
            if (table == null) throw new ArgumentNullException("table", "table is null");
            Table = table;
            TableName = table.Name;
        }

        protected abstract IProcedureWriter CreateProcedureWriter(string procName);
        protected abstract string ColumnDataType(DatabaseColumn column);
        protected abstract string ColumnDataType(string dataType);

        /// <summary>
        /// Optionally override how column parameters are formatted
        /// </summary>
        /// <value>The format parameter function.</value>
        public Func<string, string> FormatParameter { get; set; }

        private string ParameterName(string columnName)
        {
            //override how column parameters are formatted
            if (FormatParameter != null)
                columnName = FormatParameter(columnName);

            return columnName;
        }

        /// <summary>
        /// Gets or sets the manual prefix.
        /// </summary>
        /// <value>The manual prefix.</value>
        public string ManualPrefix { get; set; }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        /// <value>The suffix.</value>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix
        {
            get
            {
                if (!string.IsNullOrEmpty(ManualPrefix)) return ManualPrefix;
                return TableName + "_";
            }
        }

        /// <summary>
        /// Writes to folder.
        /// </summary>
        /// <param name="path">The path.</param>
        public void WriteToFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (!Directory.Exists(path))
                throw new ArgumentException("Path does not exist", path);

            _path = path;
            _scriptPath = null;

            Write();
        }

        /// <summary>
        /// Writes to script.
        /// </summary>
        /// <param name="scriptPath">The script path.</param>
        public void WriteToScript(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath))
                throw new ArgumentNullException("scriptPath");
            // ReSharper disable AssignNullToNotNullAttribute
            if (!Directory.Exists(Path.GetDirectoryName(scriptPath)))
                // ReSharper restore AssignNullToNotNullAttribute
                throw new ArgumentException("Path does not exist", scriptPath);

            _scriptPath = scriptPath;

            Write();
        }

        public virtual string CursorParameterName { get; set; }

        private void Write()
        {
            SqlWriter.FormatParameter = FormatParameter;

            StartWrite();

            SelectAll();
            SelectById();
            SelectPaged();
            Insert();
            Update();
            Delete();

            foreach (var fk in Table.ForeignKeys)
            {
                if (fk.Columns.Count != 1) continue;
                var name = fk.RefersToTable;
                //get rid of prefix_
                var i = name.IndexOf('_');
                if (i != -1) name = name.Substring(i + 1);
                SelectBy(name, fk.Columns[0]);
            }
            //foreach (var uq in _table.UniqueKeys)
            //{
            //also save by unique key?
            //}
            Save();
        }

        private void WriteFile(string fileName, string txt)
        {
            if (string.IsNullOrEmpty(_scriptPath))
                System.IO.File.WriteAllText(Path.Combine(_path, fileName), txt);
            else
                File.Append(txt);
        }
        protected virtual void StartWrite()
        {
        }
        protected virtual void WriteSignature(string signature)
        {
        }
        protected virtual void Save()
        {
            if (!string.IsNullOrEmpty(_scriptPath))
                System.IO.File.WriteAllText(_scriptPath, File.ToString());
        }
        private void SelectAll()
        {
            string procName = Prefix + "GETALL" + Suffix;
            string fileName = procName + ".sql";
            var writer = CreateProcedureWriter(procName);
            writer.BeginProcedure(true);
            writer.AddQuerySql(SqlWriter.SelectAllSql());
            var txt = writer.End();
            WriteFile(fileName, txt);
            WriteSignature(writer.Signature());
        }


        private void SelectById()
        {
            string procName = Prefix + "GETBYID" + Suffix;
            string fileName = procName + ".sql";
            var writer = CreateProcedureWriter(procName);
            AddPrimaryKeyParameter(writer);
            writer.BeginProcedure(true);
            writer.AddQuerySql(SqlWriter.SelectByIdSql());
            var txt = writer.End();
            WriteFile(fileName, txt);
            WriteSignature(writer.Signature());
        }

        private void SelectBy(string name, string column)
        {
            string procName = Prefix + "GETBY_" + name + Suffix;
            string fileName = procName + ".sql";
            var writer = CreateProcedureWriter(procName);
            writer.AddParameter(column, ColumnDataType("INT"));
            writer.BeginProcedure(true);
            writer.AddQuerySql(SqlWriter.SelectWhereSql(column));
            var txt = writer.End();
            WriteFile(fileName, txt);
            WriteSignature(writer.Signature());
        }

        private void SelectPaged()
        {
            string procName = Prefix + "GETPAGED" + Suffix;
            string fileName = procName + ".sql";
            var writer = CreateProcedureWriter(procName);
            writer.AddParameter(ParameterName("currentPage"), ColumnDataType("INT"));
            writer.AddParameter(ParameterName("pageSize"), ColumnDataType("INT"));
            writer.AddOutputParameter(ParameterName("total"), ColumnDataType("INT"));
            writer.BeginProcedure(true);
            //returns two result sets- paged, and count of total
            writer.AddQuerySql(SqlWriter.SelectPageSql());
            writer.AddSql(SqlWriter.CountSql(ParameterName("total")));
            var txt = writer.End();
            WriteFile(fileName, txt);
            WriteSignature(writer.Signature());
        }

        private void Insert()
        {
            string procName = Prefix + "INSERT" + Suffix;
            string fileName = procName + ".sql";
            var writer = CreateProcedureWriter(procName);
            DatabaseColumn identityColumn = null;
            DatabaseColumn rowVersionColumn = null;
            foreach (var column in Table.Columns)
            {
                if (column.IsIdentity) //don't insert identity
                {
                    identityColumn = column;
                    continue;
                }
                if (column.IsTimestamp()) //don't insert timestamp
                {
                    rowVersionColumn = column;
                    continue;
                }

                writer.AddParameter(ParameterName(column.Name), ColumnDataType(column));
            }
            if (rowVersionColumn != null)
                writer.AddOutputParameter(ParameterName(rowVersionColumn.Name), ColumnDataType("TIMESTAMP"));

            if (identityColumn != null)
                writer.AddOutputParameter(ParameterName(identityColumn.Name), ColumnDataType("INT"));

            writer.BeginProcedure();
            writer.AddSql(SqlWriter.InsertSql()); //returns scope_identity

            InsertWithRowVersionSql(writer, identityColumn, rowVersionColumn);

            var txt = writer.End();
            WriteFile(fileName, txt);
            WriteSignature(writer.Signature());
        }

        private void InsertWithRowVersionSql(IProcedureWriter writer, DatabaseColumn identityColumn, DatabaseColumn rowVersionColumn)
        {
            if (rowVersionColumn != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("SELECT ");
                sb.Append("  @" + ParameterName(rowVersionColumn.Name));
                sb.Append(" = ");
                sb.AppendLine(rowVersionColumn.Name);
                sb.Append(" FROM ");
                sb.AppendLine(SqlWriter.EscapedTableName);
                sb.AppendLine(" WHERE ");
                if (identityColumn != null)
                {
                    AddWhere(sb, identityColumn.Name);
                }
                else
                {
                    int count = SqlWriter.PrimaryKeys.Count;
                    for (int i = 0; i < count; i++)
                    {
                        if (i > 0) sb.AppendLine(" AND ");
                        var pk = SqlWriter.PrimaryKeys[i];
                        AddWhere(sb, pk);
                    }
                }
                sb.AppendLine(";");
                writer.AddSql(sb.ToString());
            }
        }

        private void AddWhere(StringBuilder sb, string pk)
        {
            sb.Append("  ");
            sb.Append(pk);
            sb.Append(" = ");
            sb.Append("@" + ParameterName(pk));
        }

        private void Update()
        {
            string procName = Prefix + "UPDATE" + Suffix;
            string fileName = procName + ".sql";
            var writer = CreateProcedureWriter(procName);
            foreach (var column in Table.Columns)
            {
                writer.AddParameter(ParameterName(column.Name), ColumnDataType(column));
            }
            writer.BeginProcedure();
            writer.AddSql(SqlWriter.UpdateSql());
            var txt = writer.End();
            WriteFile(fileName, txt);
            WriteSignature(writer.Signature());
        }

        private void Delete()
        {
            string procName = Prefix + "DELETEBYID" + Suffix;
            string fileName = procName + ".sql";
            var writer = CreateProcedureWriter(procName);
            AddPrimaryKeyParameter(writer);
            writer.BeginProcedure();
            writer.AddSql(SqlWriter.DeleteSql());
            var txt = writer.End();
            WriteFile(fileName, txt);
            WriteSignature(writer.Signature());
        }

        private void AddPrimaryKeyParameter(IProcedureWriter writer)
        {
            var pks = SqlWriter.PrimaryKeys;
            foreach (var pk in pks)
            {
                var key = pk;
                var col = Table.Columns.Find(c => c.Name == key);
                var type = ColumnDataType(col);
                writer.AddParameter(ParameterName(pk), type);
            }
        }
    }
}
