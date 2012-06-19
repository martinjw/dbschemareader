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

        public bool IncludeSchema { get; set; }

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
                                 @"ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2})",
                                 TableName(_table),
                                 EscapeName(pkName),
                                 columnList) + SqlFormatProvider().LineEnding();
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

        protected virtual string AddUniqueConstraintFormat
        {
            get { return "ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2})"; }
        }
        private string WriteUniqueKey(DatabaseConstraint uniqueKey)
        {
            var columnList = GetColumnList(uniqueKey.Columns);

            var name = ConstraintName(uniqueKey.Name);

            return string.Format(CultureInfo.InvariantCulture,
                                 AddUniqueConstraintFormat,
                                 TableName(_table),
                                 EscapeName(name),
                                 columnList) + SqlFormatProvider().LineEnding();

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
            //cannot compare empty expression
            if (string.IsNullOrEmpty(expression)) return null;
            //remove wrapping
            if (expression.StartsWith("(", StringComparison.OrdinalIgnoreCase) && expression.EndsWith(")", StringComparison.OrdinalIgnoreCase))
            {
                expression = expression.Substring(1, expression.Length - 2);
            }
            //ignore "IS NOT NULL" constraints as they are generally handled on the add/alter column level
            if (expression.EndsWith(" IS NOT NULL", StringComparison.OrdinalIgnoreCase))
                return null;

            //translate if required
            if (TranslateCheckConstraint != null) expression = TranslateCheckConstraint(expression);

            var constraintName = checkConstraint.Name;
            if (constraintName.Contains("]."))
            {
                //access format names [table].[column].ValidationRule
                constraintName = constraintName.Replace("[", "").Replace("]", "").Replace(".", "_");
                expression = checkConstraint.Name.Substring(0, checkConstraint.Name.LastIndexOf("].", StringComparison.Ordinal) + 1) + " " + expression;
            }
            var name = ConstraintName(constraintName);

            return string.Format(CultureInfo.InvariantCulture,
                                 @"ALTER TABLE {0} ADD CONSTRAINT {1} CHECK ({2})",
                                 TableName(_table),
                                 EscapeName(name),
                                 expression) + SqlFormatProvider().LineEnding();

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
            var foreignKeyTableName = ForeignKeyTableName(foreignKey);

            var fkTablePks = foreignKey.ReferencedColumns(_table.DatabaseSchema);
            //if we can't find other table, we won't list the fk table primary key columns - it *should* be automatic
            //in practice, SQLServer/Oracle are ok but MySQL will error 
            var fkColumnList = fkTablePks == null ? string.Empty : " (" + GetColumnList(fkTablePks) + ")";

            var deleteUpdateRule = string.Empty;
            if (!string.IsNullOrEmpty(foreignKey.DeleteRule))
            {
                // { CASCADE | NO ACTION | SET DEFAULT | SET NULL }
                deleteUpdateRule = " ON DELETE " + foreignKey.DeleteRule;
            }
            if (!string.IsNullOrEmpty(foreignKey.UpdateRule))
            {
                // { CASCADE | NO ACTION | SET DEFAULT | SET NULL }
                deleteUpdateRule += " ON UPDATE " + foreignKey.UpdateRule;
            }
            if (_table.Name == foreignKeyTableName 
                && !string.IsNullOrEmpty(deleteUpdateRule)
                && !IsSelfReferencingCascadeAllowed())
            {
                //SqlServer cannot have cascade rules on self-referencing tables
                deleteUpdateRule = string.Empty;
            }

            //arguably we should fully qualify the refersToTable with its schema
            return string.Format(CultureInfo.InvariantCulture,
                                 "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}{4}{5}",
                                 TableName(_table),
                                 EscapeName(foreignKey.Name),
                                 GetColumnList(foreignKey.Columns),
                                 foreignKeyTableName,
                                 fkColumnList,
                                 deleteUpdateRule) + SqlFormatProvider().LineEnding();
        }

        protected virtual bool IsSelfReferencingCascadeAllowed()
        {
            //SQLSERVER only: A foreign key constraint that has an UPDATE or a DELETE CASCADE rule, and self-references a column in the same table, is not allowed
            return true;
        }

        private string ForeignKeyTableName(DatabaseConstraint foreignKey)
        {
            var foreignKeyTable = foreignKey.ReferencedTable(_table.DatabaseSchema);

            return (foreignKeyTable != null)
                                          ? TableName(foreignKeyTable)
                                          : EscapeName(foreignKey.RefersToTable);
        }

        public string WriteConstraint(DatabaseConstraint constraint)
        {
            switch (constraint.ConstraintType)
            {
                case ConstraintType.PrimaryKey:
                    return WritePrimaryKey();
                case ConstraintType.UniqueKey:
                    return WriteUniqueKey(constraint);
                case ConstraintType.ForeignKey:
                    return WriteForeignKey(constraint);
                case ConstraintType.Check:
                    return WriteCheckConstraint(constraint);
            }
            return string.Empty;
        }

        private string TableName(DatabaseTable databaseTable)
        {
            return SchemaPrefix(databaseTable.SchemaOwner) + EscapeName(databaseTable.Name);
        }

        private string SchemaPrefix(string schema)
        {
            if (IncludeSchema && !string.IsNullOrEmpty(schema))
            {
                return EscapeName(schema) + ".";
            }
            return string.Empty;
        }

        private string ConstraintName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "CON";
            //when translating we may exceed limits
            var maximumNameLength = SqlFormatProvider().MaximumNameLength;
            if (name.Length > maximumNameLength)
            {
                name = name.Substring(0, maximumNameLength);
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
