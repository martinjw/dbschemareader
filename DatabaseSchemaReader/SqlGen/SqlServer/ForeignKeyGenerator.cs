using System;
using System.IO;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    /// <summary>
    /// Generates foreign keys
    /// </summary>
    class ForeignKeyGenerator
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
                    WriteForeignKey(table, refs, sb);
                    sb.AppendLine("GO");
                }

            }
            return sb.ToString();
        }

        private void WriteForeignKey(DatabaseTable table, DatabaseConstraint foreignKey, StringBuilder sb)
        {
            sb.AppendLine("ALTER TABLE [" + table.Name + "]");
            var cols = string.Join(", ", foreignKey.Columns.ToArray());
            var referencedTableName = foreignKey.RefersToTable;
            var referencedSchemaName = foreignKey.RefersToSchema;
            //find the referenced table's primary key
            var refTable = _schema.Tables.Find(t => t.Name == referencedTableName && t.SchemaOwner == referencedSchemaName);
            var refPrimaryKey = refTable.PrimaryKey;
            var refcols = string.Join(", ", refPrimaryKey.Columns.ToArray());

            sb.AppendLine(" ADD CONSTRAINT [" + foreignKey.Name +
                          "] FOREIGN KEY (" + cols + ") REFERENCES " + referencedTableName + "(" + refcols + ")");
        }
    }
}
