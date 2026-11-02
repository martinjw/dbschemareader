using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    internal sealed class DocRunner
    {
        private readonly DatabaseSchema _databaseSchema;

        public DocRunner(DatabaseSchema databaseSchema)
        {
            _databaseSchema = databaseSchema ?? throw new ArgumentNullException(nameof(databaseSchema));
        }

        public void Execute(DirectoryInfo directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (!directory.Exists) directory.Create();

            var outputFile = Path.Combine(directory.FullName, "database-schema.md");
            File.WriteAllText(outputFile, BuildMarkdown(), Encoding.UTF8);
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

            return sb.ToString();
        }

        private static void AppendTables(StringBuilder sb, ICollection<DatabaseTable> tables)
        {
            if (tables == null || tables.Count == 0)
            {
                sb.AppendLine("No tables found.");
                return;
            }

            foreach (var table in tables.OrderBy(t => t.SchemaOwner).ThenBy(t => t.Name, StringComparer.OrdinalIgnoreCase))
            {
                sb.AppendLine("## " + table.Name);
                sb.AppendLine("- Schema: " + Safe(table.SchemaOwner));

                if (!string.IsNullOrWhiteSpace(table.Description))
                {
                    sb.AppendLine("- Description: " + Safe(table.Description));
                }
                sb.AppendLine();

                sb.AppendLine("| Column | Type | Nullable | PK | Foreign Key | Description |");
                sb.AppendLine("|---|---|---:|---:|---|---|");

                foreach (var column in table.Columns.OrderBy(c => c.Ordinal))
                {
                    sb.AppendLine(
                        $"| {EscapePipe(column.Name)} | {EscapePipe(GetTypeDisplay(column))} | {(column.Nullable ? "✓" : string.Empty)} | {(column.IsPrimaryKey ? "🔑" : string.Empty)} | {EscapePipe(column.ForeignKeyTableName)} | {EscapePipe(column.Description)} |");
                }

                sb.AppendLine();

                AppendConstraints(sb, table);
            }
        }

        private static void AppendConstraints(StringBuilder sb, DatabaseTable table)
        {

            sb.AppendLine();
            sb.AppendLine("### Constraints");

            if (table.PrimaryKey != null)
            {
                sb.Append("- Primary Key: ");
                sb.Append(Safe(table.PrimaryKey.Name));

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
                sb.Append(Safe(fk.Name));

                var refersTo = Safe(Convert.ToString(fk.RefersToTable));
                if (!string.IsNullOrWhiteSpace(refersTo))
                {
                    sb.Append(" -> ");
                    sb.Append(refersTo);
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
                sb.Append(Safe(uk.Name));

                var ukColumns = FormatConstraintColumns(uk);
                if (!string.IsNullOrEmpty(ukColumns))
                {
                    sb.Append(" (");
                    sb.Append(ukColumns);
                    sb.Append(")");
                }

                sb.AppendLine();
            }

            sb.AppendLine();
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

            var type = Safe(column.DbDataType);
            if (column.Length.HasValue && column.Length.Value > 0)
            {
                return type + "(" + column.Length.Value + ")";
            }

            if (column.Precision.HasValue && column.Scale.HasValue)
            {
                return type + "(" + column.Precision.Value + "," + column.Scale.Value + ")";
            }

            if (column.Precision.HasValue)
            {
                return type + "(" + column.Precision.Value + ")";
            }

            return type;
        }

        private static string EscapePipe(string value)
        {
            return Safe(value).Replace("|", "\\|");
        }

        private static string Safe(string value)
        {
            return value ?? string.Empty;
        }
    }
}