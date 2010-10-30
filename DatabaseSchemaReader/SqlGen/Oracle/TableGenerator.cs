using System;
using System.IO;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Oracle
{
    public class TableGenerator : ITableGenerator
    {
        private readonly DatabaseTable _table;
        private readonly string _tableName;
        private string _path;

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

            _path = path;

            var sb = new StringBuilder();

            sb.AppendLine("CREATE TABLE [" + _tableName + "]");
            sb.AppendLine("(");
            foreach (var column in _table.Columns)
            {
                sb.Append("  [" + column.Name + "] ");
                sb.Append(WriteDataType(column));
                sb.AppendLine();
            }
            sb.AppendLine(")");

            string fileName = _tableName + ".sql";

            File.WriteAllText(Path.Combine(_path, fileName), sb.ToString());

        }

        private string WriteDataType(DatabaseColumn column)
        {
            var sql = string.Empty;
            var defaultValue = string.Empty;
            var dataType = column.DbDataType.ToUpperInvariant();
            var precision = column.Precision;
            var scale = column.Scale;

            if (dataType == "BOOLEAN")
            {
                dataType = "NUMBER";
                precision = 1;
                scale = 0;
            }
            //sql server to oracle translation
            if (dataType == "VARBINARY" || dataType == "IMAGE") dataType = "BLOB";
            if (dataType == "NVARCHAR" && column.Length> 4000) dataType = "CLOB";
            if (dataType == "NTEXT" || dataType == "TEXT") dataType = "CLOB";
            //Dates in SQL Server range from 1753 A.D. to 9999 A.D., whereas dates in Oracle range from 4712 B.C. to 4712 A.D.
            if (dataType == "DATETIME") dataType = "DATE"; 
            if (dataType == "NUMERIC") dataType = "NUMBER";
            if (dataType == "INT")
            {
                dataType = "NUMBER";
                precision = 38;
                scale = 0;
            }
            if (dataType == "BIT")
            {
                dataType = "NUMBER";
                precision = 1;
                scale = 0;
            }
            if (dataType == "DECIMAL")
            {
                dataType = "NUMBER";
                precision = 18;
                scale = 0;
            }
            if (dataType == "MONEY")
            {
                dataType = "NUMBER";
                precision = 15;
                scale = 4;
            }

            //write out Oracle datatype definition
            if (dataType == "NVARCHAR2" || dataType == "VARCHAR2")
            {
                sql = dataType + " (" + column.Length + " CHAR)";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT '" + column.DefaultValue + "'";
            }

            if (dataType == "NUMBER")
            {
                var writeScale = ((scale != null) && (scale > 0) ? "," + scale.ToString() : "");
                sql = "NUMBER (" + precision + writeScale + ")";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT " + column.DefaultValue;
            }

            if (dataType == "CHAR")
            {
                sql = "CHAR (" + column.Length + ")";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT '" + column.DefaultValue + "'";
            }

            if (dataType == "DATE")
            {
                sql = "DATE";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT DATE '" + column.DefaultValue + "'";
            }

            if (dataType == "TIMESTAMP")
            {
                sql = "TIMESTAMP" + (precision.HasValue ? " (" + precision + ")" : " (6)");
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT TIMESTAMP '" + column.DefaultValue + "'";
            }

            if (dataType == "CLOB")
            {
                sql = "CLOB ";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT '" + column.DefaultValue + "'";
            }

            if (dataType == "BLOB")
            {
                sql = "BLOB ";
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT '" + column.DefaultValue + "'";
            }

            if (string.IsNullOrEmpty(sql))
            {
                sql = column.DbDataType;
                if (!string.IsNullOrEmpty(column.DefaultValue))
                    defaultValue = " DEFAULT '" + column.DefaultValue + "'";
            }

            return sql + defaultValue + (column.Nullable ? " NOT NULL" : " NULL");
        }
    }
}
