using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen
{
    class MigrationGenerator : IMigrationGenerator
    {
        private readonly ISqlFormatProvider _sqlFormatProvider;
        private readonly DdlGeneratorFactory _ddlFactory;

        public MigrationGenerator(SqlType sqlType)
        {
            _sqlFormatProvider = SqlFormatFactory.Provider(sqlType);
            _ddlFactory = new DdlGeneratorFactory(sqlType);
        }

        protected virtual ITableGenerator CreateTableGenerator(DatabaseTable databaseTable)
        {
            return _ddlFactory.TableGenerator(databaseTable);
        }
        protected virtual ISqlFormatProvider SqlFormatProvider()
        {
            return _sqlFormatProvider;
        }

        public string Escape(string name)
        {
            return SqlFormatProvider().Escape(name);
        }
        public string LineEnding()
        {
            return SqlFormatProvider().LineEnding();
        }
        public string CreateTable(DatabaseTable databaseTable)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            return tableGenerator.Write().Trim();
        }

        public string AddColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} ADD {1};",
                Escape(databaseTable.Name),
                tableGenerator.WriteColumn(databaseColumn).Trim());
        }

        protected virtual string AlterColumnFormat
        {
            get { return "ALTER TABLE {0} MODIFY {1};"; }
        }
        protected virtual bool SupportsAlterColumn { get { return true; } }

        public string AlterColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, DatabaseColumn originalColumn)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            var columnDefinition = tableGenerator.WriteColumn(databaseColumn).Trim();
            var originalDefinition = tableGenerator.WriteColumn(originalColumn).Trim();
            //we don't specify "NULL" for nullables in tableGenerator, but if it's changed we should
            if (originalColumn.Nullable && !databaseColumn.Nullable)
            {
                originalDefinition += " NULL";
            }
            if (!originalColumn.Nullable && databaseColumn.Nullable)
            {
                columnDefinition += " NULL";
            }

            //add a nice comment
            string comment = string.Format(CultureInfo.InvariantCulture,
                "-- {0} from {1} to {2}",
                databaseTable.Name,
                originalDefinition,
                columnDefinition);
            if (!SupportsAlterColumn || databaseColumn.IsPrimaryKey || databaseColumn.IsForeignKey)
            {
                //SQLite does not have modify column
                //you can't change primary keys
                //you can't change foreign key columns
                return comment + Environment.NewLine + "-- TODO: change manually";
            }

            //there are practical restrictions on what can be altered
            //* changing null to not null will fail if the table column data contains nulls
            //* you can't change between incompatible datatypes
            //* you can't change datatypes if there is a default value (but you can change length/precision/scale)
            //* you can't change datatypes if column used in indexes (incl. primary keys and foreign keys)
            //* and so on...
            //

            return comment +
                Environment.NewLine +
                string.Format(CultureInfo.InvariantCulture,
                    AlterColumnFormat,
                    Escape(databaseTable.Name),
                    columnDefinition);
        }

        protected virtual string AddUniqueConstraintFormat
        {
            get { return "ALTER TABLE {0} ADD CONSTRAINT {1} UNIQUE ({2});"; }
        }
        public virtual string AddConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            //we always use the named form.
            var constraintName = constraint.Name;

            if (string.IsNullOrEmpty(constraintName)) throw new InvalidOperationException("Constraint must have a name");
            if (constraint.Columns.Count == 0) throw new InvalidOperationException("Constraint has no columns");

            if (constraint.ConstraintType == ConstraintType.PrimaryKey)
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     "ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2});",
                                     Escape(databaseTable.Name),
                                     Escape(constraintName),
                                     GetColumnList(constraint.Columns));
            }
            if (constraint.ConstraintType == ConstraintType.UniqueKey)
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     AddUniqueConstraintFormat,
                                     Escape(databaseTable.Name),
                                     Escape(constraintName),
                                     GetColumnList(constraint.Columns));
            }
            if (constraint.ConstraintType == ConstraintType.ForeignKey)
            {
                var fkTablePks = constraint.ReferencedColumns(databaseTable.DatabaseSchema);
                //if we can't find other table, we won't list the fk table primary key columns - it *should* be automatic
                //in practice, SQLServer/Oracle are ok but MySQL will error 
                var fkColumnList = fkTablePks == null ? string.Empty : " (" + GetColumnList(fkTablePks) + ")";

                return string.Format(CultureInfo.InvariantCulture,
                                     "ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3}{4};",
                                     Escape(databaseTable.Name),
                                     Escape(constraintName),
                                     GetColumnList(constraint.Columns),
                                     Escape(constraint.RefersToTable),
                                     fkColumnList);
            }
            if (constraint.ConstraintType == ConstraintType.Check)
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     "ALTER TABLE {0} ADD CONSTRAINT {1} CHECK ({2});",
                                     Escape(databaseTable.Name),
                                     Escape(constraintName),
                                     constraint.Expression);
            }
            return null;
        }
        public virtual string DropConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            if (constraint.ConstraintType == ConstraintType.UniqueKey)
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     DropUniqueFormat,
                                     Escape(databaseTable.Name),
                                     Escape(constraint.Name));
            }
            return string.Format(CultureInfo.InvariantCulture,
                                 DropForeignKeyFormat,
                                 Escape(databaseTable.Name),
                                 Escape(constraint.Name));
        }
        protected virtual string DropForeignKeyFormat
        {
            get { return "ALTER TABLE {0} DROP CONSTRAINT {1};"; }
        }
        protected virtual string DropUniqueFormat
        {
            get { return DropForeignKeyFormat; }
        }

        protected virtual bool SupportsDropColumn { get { return true; } }
        public string DropColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            if (!SupportsDropColumn)
            {
                //SQLite does not have drop or modify column. We could create the new table and select into it, but not on this first version
                return "-- " + databaseTable.Name + " column " + databaseColumn.Name + " should be dropped";
            }
            var sb = new StringBuilder();
            if (databaseColumn.IsForeignKey)
            {
                foreach (var foreignKey in databaseTable.ForeignKeys)
                {
                    if (!foreignKey.Columns.Contains(databaseColumn.Name)) continue;

                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        DropForeignKeyFormat,
                        Escape(databaseTable.Name),
                        Escape(foreignKey.Name)));
                }
            }
            if (databaseColumn.IsUniqueKey)
            {
                foreach (var uniqueKey in databaseTable.UniqueKeys)
                {
                    if (!uniqueKey.Columns.Contains(databaseColumn.Name)) continue;
                    sb.AppendLine(string.Format(CultureInfo.InvariantCulture,
                        DropForeignKeyFormat,
                        Escape(databaseTable.Name),
                        Escape(uniqueKey.Name)));
                }
            }
            sb.AppendLine("ALTER TABLE " + Escape(databaseTable.Name) + " DROP COLUMN " + Escape(databaseColumn.Name) + ";");
            return sb.ToString();
        }

        public virtual string DropTable(DatabaseTable databaseTable)
        {
            var tableName = Escape(databaseTable.Name);
            var sb = new StringBuilder();
            foreach (var foreignKey in databaseTable.ForeignKeys)
            {
                sb.AppendLine("ALTER TABLE " + tableName + " DROP CONSTRAINT " + Escape(foreignKey.Name) + ";");
            }
            sb.AppendLine("DROP TABLE " + tableName + ";");
            return sb.ToString();
        }

        private string GetColumnList(IEnumerable<string> columns)
        {
            var escapedColumnNames = columns.Select(column => Escape(column)).ToArray();
            return string.Join(", ", escapedColumnNames);
        }
    }
}
