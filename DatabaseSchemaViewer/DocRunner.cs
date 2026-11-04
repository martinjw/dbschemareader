using DatabaseSchemaReader.DataSchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DatabaseSchemaViewer
{
    internal sealed class DocRunner
    {
        private const int SplitTablesThreshold = 100;
        private const string TablesFolderName = "tables";

        private readonly DatabaseSchema _databaseSchema;
        private bool _splitTables;
        private bool _writingTableFile;
        private IDictionary<string, string> _tableFiles;

        public DocRunner(DatabaseSchema databaseSchema)
        {
            _databaseSchema = databaseSchema ?? throw new ArgumentNullException(nameof(databaseSchema));
        }

        public string Execute(DirectoryInfo directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (!directory.Exists) directory.Create();

            _splitTables = (_databaseSchema.Tables?.Count ?? 0) >= SplitTablesThreshold;
            _tableFiles = _splitTables ? BuildTableFileMap(_databaseSchema.Tables) : null;

            if (_splitTables)
            {
                WriteTables(directory, _databaseSchema.Tables);
            }

            var outputFile = Path.Combine(directory.FullName, "database-schema.md");
            File.WriteAllText(outputFile, BuildMarkdown(), Encoding.UTF8);
            return outputFile;
        }

        private void AppendConstraints(StringBuilder sb, DatabaseTable table)
        {
            sb.AppendLine();
            sb.AppendLine("### Constraints");

            if (table.PrimaryKey != null)
            {
                sb.Append("- Primary Key: ");
                sb.Append($"**{Safe(table.PrimaryKey.Name)}**");

                var pkColumns = FormatConstraintColumns(table.PrimaryKey);
                if (!string.IsNullOrEmpty(pkColumns))
                {
                    sb.Append(" (");
                    sb.Append(pkColumns);
                    sb.Append(")");
                }

                sb.AppendLine();
            }

            foreach (var fk in table.ForeignKeys)
            {
                sb.Append("- Foreign Key: ");
                sb.Append($"**{Safe(fk.Name)}**");

                if (!string.IsNullOrWhiteSpace(fk.RefersToTable))
                {
                    sb.Append(" -> ");
                    AppendMarkdownAnchor(sb, fk.RefersToTable, ResolveTableFileName(fk.RefersToTable));
                }

                var fkColumns = FormatConstraintColumns(fk);
                if (!string.IsNullOrEmpty(fkColumns))
                {
                    sb.Append(" (");
                    sb.Append(fkColumns);
                    sb.Append(")");
                }

                sb.AppendLine();
            }

            foreach (var uk in table.UniqueKeys)
            {
                sb.Append("- Unique Key: ");
                sb.Append($"**{Safe(uk.Name)}**");

                var ukColumns = FormatConstraintColumns(uk);
                if (!string.IsNullOrEmpty(ukColumns))
                {
                    sb.Append(" (");
                    sb.Append(ukColumns);
                    sb.Append(")");
                }

                sb.AppendLine();
            }

            foreach (var ck in table.CheckConstraints)
            {
                sb.Append("- Check constraint: ");
                sb.Append($"**{Safe(ck.Name)}**");

                sb.Append(" -> [`");
                sb.Append(ck.Expression);
                sb.Append("`]");

                sb.AppendLine();
            }

            sb.AppendLine();
        }

        private static void AppendIndexes(StringBuilder sb, DatabaseTable table)
        {
            sb.AppendLine("### Indexes");

            var indexes = table.Indexes;
            if (indexes == null || indexes.Count == 0)
            {
                sb.AppendLine("- None");
                sb.AppendLine();
                return;
            }

            foreach (var index in indexes.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase))
            {
                var indexName = Safe(index.Name);
                var columns = index.Columns;
                var isUnique = index.IsUnique;

                sb.Append("- ");
                sb.Append("**");
                sb.Append(indexName);
                sb.Append("**");

                if (isUnique)
                {
                    sb.Append(" (Unique)");
                }

                if (columns.Count > 0)
                {
                    sb.Append(": ");
                    sb.Append(string.Join(", ", columns.Select(x => Safe(x.Name))));
                }

                sb.AppendLine();
            }

            sb.AppendLine();
        }

        private static void AppendMarkdownAnchor(StringBuilder sb, string headingText, string fileName = null)
        {
            if (string.IsNullOrWhiteSpace(headingText))
            {
                return;
            }

            sb.Append("[`");
            sb.Append(headingText);
            sb.Append("`](");
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                sb.Append(fileName);
            }
            sb.Append("#");
            sb.Append(headingText.ToLowerInvariant().Replace(" ", "-"));
            sb.Append(")");
        }

        private void AppendTable(StringBuilder sb, DatabaseTable table)
        {
            sb.AppendLine("## " + table.Name);
            sb.AppendLine("- Schema: " + Safe(table.SchemaOwner));

            if (!string.IsNullOrWhiteSpace(table.Description))
            {
                sb.AppendLine($"- Description: `{Safe(table.Description)}`");
            }
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine("| Column | Type | Nullable | PK | Foreign Key | Description |");
            sb.AppendLine("|---|---|---:|---:|---|---|");

            foreach (var column in table.Columns.OrderBy(c => c.Ordinal))
            {
                var description = column.Description;
                if (!string.IsNullOrWhiteSpace(column.DefaultValue))
                {
                    description += $" Default: `{column.DefaultValue}`";
                }

                sb.Append("| **");
                sb.Append(EscapePipe(column.Name));
                sb.Append("** | ");
                sb.Append(EscapePipe(GetTypeDisplay(column)));
                sb.Append(" | ");
                sb.Append(column.Nullable ? "✓" : string.Empty);
                sb.Append(" | ");
                sb.Append(column.IsPrimaryKey ? "🔑" : string.Empty);
                sb.Append(column.IsAutoNumber ? "🤖" : string.Empty);
                sb.Append(" | ");
                if (column.ForeignKeyTableName != null)
                {
                    AppendMarkdownAnchor(sb, column.ForeignKeyTableName, ResolveTableFileName(column.ForeignKeyTableName));
                }
                sb.Append(" | ");
                sb.Append(EscapePipe(description));
                sb.AppendLine(" |");
            }

            sb.AppendLine();

            AppendConstraints(sb, table);
            AppendIndexes(sb, table);
            AddReferences(sb, table);
            //to do: triggers
        }

        private void AddReferences(StringBuilder sb, DatabaseTable table)
        {
            var refs = table.ForeignKeyChildren;
            if (refs == null || refs.Count == 0)
            {
                return;
            }
            sb.AppendLine("### References");
            foreach (var refTable in refs)
            {
                sb.Append("- ");
                AppendMarkdownAnchor(sb, refTable.Name, ResolveTableFileName(refTable.Name));
                sb.AppendLine();
            }
        }

        private void AppendTables(StringBuilder sb, ICollection<DatabaseTable> tables)
        {
            if (tables == null || tables.Count == 0)
            {
                sb.AppendLine("No tables found.");
                return;
            }

            sb.AppendLine("# Tables");
            if (_splitTables)
            {
                foreach (var table in tables.OrderBy(t => t.SchemaOwner).ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase))
                {
                    sb.Append("- ");
                    AppendMarkdownAnchor(sb, table.Name, ResolveTableFileName(GetTableKey(table)));
                    sb.AppendLine();
                }
                sb.AppendLine();
                return;
            }

            foreach (var table in tables.OrderBy(t => t.SchemaOwner).ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase))
            {
                AppendTable(sb, table);
            }
        }

        private string ResolveTableFileName(string tableName)
        {
            if (_tableFiles == null || string.IsNullOrWhiteSpace(tableName))
            {
                return null;
            }

            if (!_tableFiles.TryGetValue(tableName, out var fileName) || string.IsNullOrWhiteSpace(fileName))
            {
                return null;
            }

            if (_writingTableFile)
            {
                var prefix = TablesFolderName + "/";
                if (fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return fileName.Substring(prefix.Length);
                }
            }

            return fileName;
        }

        private static string GetTableKey(DatabaseTable table)
        {
            var schema = Safe(table?.SchemaOwner);
            var name = Safe(table?.Name);
            return string.IsNullOrWhiteSpace(schema) ? name : schema + "." + name;
        }

        private static IDictionary<string, string> BuildTableFileMap(ICollection<DatabaseTable> tables)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (tables == null || tables.Count == 0)
            {
                return map;
            }

            var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var table in tables.OrderBy(t => t.SchemaOwner).ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase))
            {
                var key = GetTableKey(table);
                var fileName = MakeUniqueFileName(MakeSafeFileName(key) + ".md", usedFileNames);
                var relativePath = TablesFolderName + "/" + fileName;

                map[key] = relativePath;
                if (!map.ContainsKey(table.Name))
                {
                    map[table.Name] = relativePath;
                }
            }

            return map;
        }

        private static string MakeUniqueFileName(string fileName, ISet<string> usedFileNames)
        {
            if (usedFileNames.Add(fileName))
            {
                return fileName;
            }

            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var i = 2;
            string candidate;
            do
            {
                candidate = baseName + "-" + i + extension;
                i++;
            }
            while (!usedFileNames.Add(candidate));

            return candidate;
        }

        private static string MakeSafeFileName(string value)
        {
            var safe = Safe(value);
            var invalidChars = Path.GetInvalidFileNameChars();
            var chars = safe.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray();
            var fileName = new string(chars).Trim();
            fileName = fileName.Replace(" ", "-"); //space in markdown is a dash
            return string.IsNullOrWhiteSpace(fileName) ? "table" : fileName;
        }

        private void WriteTables(DirectoryInfo directory, ICollection<DatabaseTable> tables)
        {
            if (tables == null || tables.Count == 0 || _tableFiles == null)
            {
                return;
            }

            var tablesDirectoryPath = Path.Combine(directory.FullName, TablesFolderName);
            Directory.CreateDirectory(tablesDirectoryPath);

            _writingTableFile = true;
            try
            {
                foreach (var table in tables.OrderBy(t => t.SchemaOwner).ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase))
                {
                    var key = GetTableKey(table);
                    if (!_tableFiles.TryGetValue(key, out var relativePath) || string.IsNullOrWhiteSpace(relativePath))
                    {
                        continue;
                    }

                    var tableFilePath = Path.Combine(directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
                    var sb = new StringBuilder();
                    AppendTable(sb, table);
                    File.WriteAllText(tableFilePath, sb.ToString(), Encoding.UTF8);
                }
            }
            finally
            {
                _writingTableFile = false;
            }
        }

        private static string EscapePipe(string value)
        {
            return Safe(value).Replace("|", "\\|");
        }

        private static string FormatConstraintColumns(DatabaseConstraint constraint)
        {
            if (constraint == null || constraint.Columns == null)
            {
                return string.Empty;
            }

            var columns = constraint.Columns
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => Safe(c))
                .ToArray();

            if (columns.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(", ", columns);
        }

        private static string GetTypeDisplay(DatabaseColumn column)
        {
            if (column == null) return string.Empty;

            var type = Safe(column.DataTypeDefinition());

            return type;
        }

        private static string Safe(string value)
        {
            return value ?? string.Empty;
        }

        private void AppendView(StringBuilder sb, DatabaseView view)
        {
            sb.AppendLine("## " + view.Name);
            sb.AppendLine("- Schema: " + Safe(view.SchemaOwner));
            sb.AppendLine("- **View** ");

            if (!string.IsNullOrWhiteSpace(view.Description))
            {
                sb.AppendLine($"- Description: `{Safe(view.Description)}`");
            }
            sb.AppendLine();
            sb.AppendLine("| Column | Type | Nullable |");
            sb.AppendLine("|---|---|---:|");
            foreach (var column in view.Columns.OrderBy(c => c.Ordinal))
            {
                sb.Append("| **");
                sb.Append(EscapePipe(column.Name));
                sb.Append("** | ");
                sb.Append(EscapePipe(GetTypeDisplay(column)));
                sb.Append(" | ");
                sb.Append(column.Nullable ? "✓" : string.Empty);
                sb.AppendLine(" |");
            }
        }

        private void AppendViews(StringBuilder sb, IList<DatabaseView> views)
        {
            if (views == null || views.Count == 0)
            {
                sb.AppendLine("No views found.");
                return;
            }

            sb.AppendLine("# Views");
            foreach (var view in views.OrderBy(v => v.SchemaOwner).ThenBy(v => v.Name, StringComparer.OrdinalIgnoreCase))
            {
                AppendView(sb, view);
            }
        }

        private string BuildMarkdown()
        {
            var sb = new StringBuilder();

            sb.AppendLine("# Database Schema");
            sb.AppendLine();
            sb.AppendLine("- Generated: " + DateTime.Now.ToString("u"));
            sb.AppendLine("- Database: " + Safe(_databaseSchema.ConnectionString));
            sb.AppendLine("- Tables: " + (_databaseSchema.Tables?.Count ?? 0));
            sb.AppendLine();

            AppendTables(sb, _databaseSchema.Tables);

            AppendViews(sb, _databaseSchema.Views);

            //to do: procedures, functions, packages, sequences?

            return sb.ToString();
        }
    }
}