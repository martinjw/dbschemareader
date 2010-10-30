using System;
using System.IO;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;

namespace Library.Data.SqlGen.SqlServer
{
    public class ForeignKeyGenerator
    {
        private readonly DatabaseSchema _schema;

        public ForeignKeyGenerator(DatabaseSchema schema)
        {
            _schema = schema;
        }


        public void WriteToFolder(string path, IEnumerable<DatabaseTable> tables)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");
            if (!Directory.Exists(path))
                throw new ArgumentException("Path does not exist", path);

            var txt = Write(tables);
            string fileName = "ForeignKeys.sql";

            File.WriteAllText(Path.Combine(path, fileName), txt);
        }

        public void WriteToScript(string scriptPath, IEnumerable<DatabaseTable> tables)
        {
            if (string.IsNullOrEmpty(scriptPath))
                throw new ArgumentNullException("scriptPath");
            if (!Directory.Exists(Path.GetDirectoryName(scriptPath)))
                throw new ArgumentException("Path does not exist", scriptPath);

            var txt = Write(tables);

            File.AppendAllText(scriptPath, txt);
        }

        private string Write(IEnumerable<DatabaseTable> tables)
        {
            var sb = new StringBuilder();
            foreach (var table in tables)
            {
                foreach (var refs in table.ForeignKeys)
                {
                    sb.AppendLine();
                    sb.AppendLine("ALTER TABLE [" + table.Name + "]");
                    var cols = string.Join(", ", refs.Columns.ToArray());
                    var refTable = _schema.Tables.Find(t => t.Name == refs.RefersToTable);
                    var refPrimaryKey = refTable.PrimaryKey;
                    var refcols = string.Join(", ", refPrimaryKey.Columns.ToArray());

                    sb.AppendLine(" ADD CONSTRAINT [" + refs.Name +
                        "] FOREIGN KEY (" + cols + ") REFERENCES " + refs.RefersToTable + "(" + refcols + ")");
                    sb.AppendLine("GO");
                }

            }
            return sb.ToString();
        }

    }
}
