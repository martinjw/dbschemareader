using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    abstract class ConstraintWriterBase
    {
        private readonly DatabaseTable _table;

        protected ConstraintWriterBase(DatabaseTable table)
        {
            _table = table;
        }

        protected abstract ISqlFormatProvider SqlFormatProvider();

        private string EscapeName(string name)
        {
            return SqlFormatProvider().Escape(name);
        }
        private string LineEnding()
        {
            return SqlFormatProvider().LineEnding();
        }
        public bool IncludeSchema { get; set; }

        protected virtual int MaximumNameLength
        {
            get { return 128; }
        }

        public Func<DatabaseConstraint, bool> CheckConstraintExcluder { get; set; }
        public Func<string, string> TranslateCheckConstraint { get; set; }

        /// <summary>
        /// Writes the table-specific constraints (primary key, unique, constraint)
        /// </summary>
        /// <returns></returns>
        public string WriteTableConstraints()
        {
            var sb = new StringBuilder();
            sb.AppendLine(WritePrimaryKey());
            sb.AppendLine(WriteUniqueKeys());
            sb.AppendLine(WriteCheckConstraints());
            return sb.ToString();
        }

        public string WritePrimaryKey()
        {
            if (_table.PrimaryKey == null) return null;
            var columnList = GetColumnList(_table.PrimaryKey.Columns);

            var pkName = ConstraintName(_table.PrimaryKey.Name);

            return string.Format(CultureInfo.InvariantCulture,
                                 @"ALTER TABLE {0}{1} 
ADD CONSTRAINT {2} PRIMARY KEY ({3})",
                                 IncludeSchema ? EscapeName(_table.SchemaOwner) + "." : string.Empty,
                                 EscapeName(_table.Name),
                                 EscapeName(pkName),
                                 columnList) + LineEnding();

        }

        public string WriteUniqueKeys()
        {
            var sb = new StringBuilder();
            foreach (var uniqueKey in _table.UniqueKeys)
            {
                sb.AppendLine(WriteUniqueKey(uniqueKey));
            }
            return sb.ToString();
        }

        private string WriteUniqueKey(DatabaseConstraint uniqueKey)
        {
            var columnList = GetColumnList(uniqueKey.Columns);

            var name = ConstraintName(uniqueKey.Name);

            return string.Format(CultureInfo.InvariantCulture,
                                 @"ALTER TABLE {0}{1} 
ADD CONSTRAINT {2} UNIQUE ({3})",
                                 IncludeSchema ? EscapeName(_table.SchemaOwner) + "." : string.Empty,
                                 EscapeName(_table.Name),
                                 EscapeName(name),
                                 columnList) + LineEnding();

        }

        public string WriteCheckConstraints()
        {
            var sb = new StringBuilder();
            foreach (var checkConstraint in _table.CheckConstraints)
            {
                sb.AppendLine(WriteCheckConstraint(checkConstraint));
            }
            return sb.ToString();
        }

        private string WriteCheckConstraint(DatabaseConstraint checkConstraint)
        {
            if (CheckConstraintExcluder != null && CheckConstraintExcluder(checkConstraint)) return null;
            var expression = checkConstraint.Expression;
            //remove wrapping
            if (expression.StartsWith("(") && expression.EndsWith(")"))
            {
                expression = expression.Substring(1, expression.Length - 2);
            }
            //translate if required
            if (TranslateCheckConstraint != null) expression = TranslateCheckConstraint(expression);

            var name = ConstraintName(checkConstraint.Name);

            return string.Format(CultureInfo.InvariantCulture,
                                 @"ALTER TABLE {0}{1} 
ADD CONSTRAINT {2} CHECK ({3})",
                                 IncludeSchema ? EscapeName(_table.SchemaOwner) + "." : string.Empty,
                                 EscapeName(_table.Name),
                                 EscapeName(name),
                                 expression) + LineEnding();

        }

        public string WriteForeignKeys()
        {
            var sb = new StringBuilder();
            foreach (var foreignKey in _table.ForeignKeys)
            {
                sb.AppendLine(WriteForeignKey(foreignKey));
            }
            return sb.ToString();
        }

        private string WriteForeignKey(DatabaseConstraint foreignKey)
        {
            var columnList = GetColumnList(foreignKey.Columns);
            //table must have schema so we can navigate to the referenced table
            if (_table.DatabaseSchema == null)
                throw new InvalidOperationException("Cannot navigate databaseSchema to find referenced tables");

            var referencedTable = foreignKey.ReferencedTable(_table.DatabaseSchema);
            //can't find the table. Don't write the fk reference.
            if (referencedTable == null) return null;

            var refColumnList = GetColumnList(referencedTable.PrimaryKey.Columns);

            var foreignKeyName = ConstraintName(foreignKey.Name);

            return string.Format(CultureInfo.InvariantCulture,
                                 @"ALTER TABLE {0}{1} 
ADD CONSTRAINT {2} FOREIGN KEY 
({3}) 
REFERENCES {4} ({5})",
                                 IncludeSchema ? EscapeName(_table.SchemaOwner) + "." : string.Empty,
                                 EscapeName(_table.Name),
                                 EscapeName(foreignKeyName),
                                 columnList,
                                 EscapeName(foreignKey.RefersToTable),
                                 refColumnList) + LineEnding();

        }

        private string ConstraintName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "CON";
            //when translating we may exceed limits
            if (name.Length > MaximumNameLength)
            {
                name = name.Substring(0, MaximumNameLength);
            }
            return name;
        }

        private string GetColumnList(IEnumerable<string> columns)
        {
            var escapedColumnNames = columns.Select(column => EscapeName(column)).ToArray();
            return string.Join(", ", escapedColumnNames);
        }
    }
}
