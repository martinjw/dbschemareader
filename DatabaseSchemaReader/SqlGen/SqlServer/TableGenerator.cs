using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    public class TableGenerator : ITableGenerator
    {
        private readonly DatabaseTable _table;
        private readonly string _tableName;
        private bool _hasBit;

        public TableGenerator(DatabaseTable table)
        {
            _table = table;
            _tableName = table.Name;
        }


        public void WriteToFolder(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (!Directory.Exists(path))
                throw new ArgumentException("Path does not exist", path);

            var w = Write();

            string fileName = _tableName + ".sql";
            File.WriteAllText(Path.Combine(path, fileName), w);
        }

        public void WriteToScript(string scriptPath)
        {
            if (string.IsNullOrEmpty(scriptPath))
                throw new ArgumentNullException("scriptPath");
            if (!Directory.Exists(Path.GetDirectoryName(scriptPath)))
                throw new ArgumentException("Path does not exist", scriptPath);

            var w = Write();

            File.AppendAllText(scriptPath, w);
        }

        private string Write()
        {
            var sb = new StringBuilder();

            sb.AppendLine("CREATE TABLE [" + _tableName + "]");
            sb.AppendLine("(");
            var list = new List<string>();
            foreach (var column in _table.Columns)
            {
                list.Add("  [" + column.Name + "] "
                + WriteDataType(column));
            }
            sb.Append(string.Join("," + Environment.NewLine, list.ToArray()));

            sb.AppendLine(")");
            sb.AppendLine("GO");
            foreach (var check in _table.CheckConstraints)
            {
                //looks like a boolean check, skip it
                if (_hasBit && check.Expression.Contains(" IN (0, 1)")) continue;

                sb.AppendLine();
                sb.AppendLine("ALTER TABLE [" + _tableName + "]");
                sb.AppendLine(" ADD CONSTRAINT [" + check.Name +
                    "] CHECK  (" + check.Expression + ")");
                sb.AppendLine("GO");
            }
            foreach (var uniques in _table.UniqueKeys)
            {
                sb.AppendLine();
                sb.AppendLine("ALTER TABLE [" + _tableName + "]");
                var cols = string.Join(", ", uniques.Columns.ToArray());

                sb.AppendLine(" ADD CONSTRAINT [" + uniques.Name +
                    "] UNIQUE (" + cols + ")");
                sb.AppendLine("GO");
            }
            return sb.ToString();
        }

        private string WriteDataType(DatabaseColumn column)
        {

            var defaultValue = string.Empty;
            if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                var defaultConstraint = " CONSTRAINT [DF_" + _tableName + "_" + column.Name + "] DEFAULT ";
                var dataType = column.DbDataType.ToUpperInvariant();
                if (dataType == "NVARCHAR2" || dataType == "VARCHAR2" || dataType == "CHAR")
                {
                    defaultValue = defaultConstraint + "'" + column.DefaultValue + "'";
                }
                else //numeric default
                {
                    defaultValue = defaultConstraint + column.DefaultValue;
                }
            }

            var sql = column.SqlServerDataType();
            if (sql == "BIT") _hasBit = true;

            if (_table.Triggers.Count == 1 && column.IsPrimaryKey)
            {
                column.IsIdentity = true;
                //if a table has a trigger, we assume it's an Oracle trigger/sequence which is translated to identity for the pk
            }
            if (column.IsIdentity) sql += " IDENTITY(1,1)";
            if (column.IsPrimaryKey) 
                sql += " PRIMARY KEY NOT NULL";
            else
                sql += " " + (column.Nullable ? " NOT NULL" : " NULL") + " " + defaultValue;
            return sql;
        }
    }
}
