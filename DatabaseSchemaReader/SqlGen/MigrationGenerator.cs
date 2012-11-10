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
            IncludeSchema = (sqlType != SqlType.SqlServerCe && sqlType != SqlType.SQLite);
        }

        /// <summary>
        /// Include the schema when writing table. Must not be set for SQLite as there is no schema.
        /// </summary>
        public bool IncludeSchema { get; set; }

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
        protected virtual string LineEnding()
        {
            return SqlFormatProvider().LineEnding();
        }
        public string AddTable(DatabaseTable databaseTable)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            tableGenerator.IncludeSchema = IncludeSchema; //cascade our setting
            return tableGenerator.Write().Trim();
        }

        public virtual string AddColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            var addColumn = tableGenerator.WriteColumn(databaseColumn).Trim();
            if (string.IsNullOrEmpty(databaseColumn.DefaultValue) && !databaseColumn.Nullable)
            {
                var dt = databaseColumn.DataType;
                if (dt == null || dt.IsString)
                {
                    addColumn += " DEFAULT ''"; //empty string
                }
                else if (dt.IsNumeric)
                {
                    addColumn += " DEFAULT 0";
                }
                else if (dt.IsDateTime)
                {
                    addColumn += " DEFAULT CURRENT_TIMESTAMP";
                }
                //make sure the NOT NULL is AFTER the default
                addColumn = addColumn.Replace(" NOT NULL ", " ") + " NOT NULL";
            }
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} ADD {1}",
                TableName(databaseTable),
                addColumn) + LineEnding();
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
            string originalDefinition = null;
            if (originalColumn != null)
            {
                originalDefinition = tableGenerator.WriteColumn(originalColumn).Trim();
                //we don't specify "NULL" for nullables in tableGenerator, but if it's changed we should
                if (originalColumn.Nullable && !databaseColumn.Nullable)
                {
                    originalDefinition += " NULL";
                }
                if (!originalColumn.Nullable && databaseColumn.Nullable)
                {
                    columnDefinition += " NULL";
                }
            }

            //add a nice comment
            string comment = string.Format(CultureInfo.InvariantCulture,
                "-- {0} from {1} to {2}",
                databaseTable.Name,
                originalDefinition,
                columnDefinition);
            if (!SupportsAlterColumn)
            {
                //SQLite does not have modify column
                return comment + Environment.NewLine + "-- TODO: change manually (no ALTER COLUMN)";
            }
            if (databaseColumn.IsPrimaryKey || databaseColumn.IsForeignKey)
            {
                //you can't change primary keys
                //you can't change foreign key columns
                return comment + Environment.NewLine + "-- TODO: change manually (PK or FK)";
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
                    TableName(databaseTable),
                    columnDefinition);
        }

        /// <summary>
        /// Renames the column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="originalColumnName">The original column name.</param>
        /// <returns></returns>
        public virtual string RenameColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, string originalColumnName)
        {
            return "--TODO rename column " + TableName(databaseTable) + " from " + originalColumnName + " to " + Escape(databaseColumn.Name);
        }

        /// <summary>
        /// Standard "Rename Column x To y" syntax for those that support it.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="originalColumnName">Name of the original column.</param>
        /// <returns></returns>
        protected string RenameColumnTo(DatabaseTable databaseTable, DatabaseColumn databaseColumn, string originalColumnName)
        {
            if (string.IsNullOrEmpty(originalColumnName) || databaseColumn == null)
                return RenameColumn(databaseTable, databaseColumn, originalColumnName);
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} RENAME COLUMN {1} TO {2}",
                TableName(databaseTable),
                Escape(originalColumnName),
                Escape(databaseColumn.Name)) + LineEnding();
        }


        public virtual string AddConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            //we always use the named form.
            var constraintName = constraint.Name;

            if (string.IsNullOrEmpty(constraintName))
                throw new InvalidOperationException("Constraint must have a name");
            //primary, unique and foreign key constraints must have columns
            if (constraint.Columns.Count == 0 && constraint.ConstraintType != ConstraintType.Check)
                throw new InvalidOperationException("Constraint has no columns");

            //use the standard constraint writer for the database
            var constraintWriter = _ddlFactory.ConstraintWriter(databaseTable);
            constraintWriter.IncludeSchema = IncludeSchema; //cascade setting
            return constraintWriter.WriteConstraint(constraint);
        }

        public virtual string DropConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            if (constraint.ConstraintType == ConstraintType.UniqueKey)
            {
                return string.Format(CultureInfo.InvariantCulture,
                                     DropUniqueFormat,
                                     TableName(databaseTable),
                                     Escape(constraint.Name)) + LineEnding();
            }
            return string.Format(CultureInfo.InvariantCulture,
                                 DropForeignKeyFormat,
                                 TableName(databaseTable),
                                 Escape(constraint.Name)) + LineEnding();
        }

        public string AddView(DatabaseView view)
        {
            //CREATE VIEW cannot be combined with other statements in a batch, so be preceeded by and terminate with a "GO" (sqlServer) or "/" (Oracle)
            var sql = view.Sql;
            if (string.IsNullOrEmpty(sql))
            {
                //without the sql, we can't do anything
                return "-- add view " + view.Name;
            }
            if (sql.TrimStart().StartsWith("CREATE VIEW ", StringComparison.OrdinalIgnoreCase))
            {
                //helpfully, SqlServer includes the create statement
                return sql + _sqlFormatProvider.RunStatements();
            }

            //Oracle and MySql have CREATE OR REPLACE
            var addView = "CREATE VIEW " + SchemaPrefix(view.SchemaOwner) + Escape(view.Name) + " AS " + sql;
            return addView + _sqlFormatProvider.RunStatements();
        }

        public string DropView(DatabaseView view)
        {
            return "DROP VIEW " + SchemaPrefix(view.SchemaOwner) + Escape(view.Name) + ";"
                + _sqlFormatProvider.RunStatements();
        }

        public virtual string AddProcedure(DatabaseStoredProcedure procedure)
        {
            //CREATE PROCEDURE cannot be combined with other statements in a batch, so be preceeded by and terminate with a "GO" (sqlServer) or "/" (Oracle)
            var sql = procedure.Sql;
            if (string.IsNullOrEmpty(sql))
            {
                //without the sql, we can't do anything
                return "-- add procedure " + procedure.Name;
            }
            if (sql.TrimStart().StartsWith("PROCEDURE ", StringComparison.OrdinalIgnoreCase))
            {
                return "CREATE " + sql + _sqlFormatProvider.RunStatements();
            }
            //helpfully, SqlServer includes the create statement
            //MySQL doesn't, so this will need to be overridden
            return sql + _sqlFormatProvider.RunStatements();
        }

        public virtual string DropProcedure(DatabaseStoredProcedure procedure)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "DROP PROCEDURE {0}{1};",
                SchemaPrefix(procedure.SchemaOwner),
                Escape(procedure.Name))
                + _sqlFormatProvider.RunStatements();
        }


        public virtual string DropFunction(DatabaseFunction databaseFunction)
        {
            return "DROP FUNCTION " + SchemaPrefix(databaseFunction.SchemaOwner) + Escape(databaseFunction.Name) + ";"
                + _sqlFormatProvider.RunStatements();
        }

        public virtual string AddFunction(DatabaseFunction databaseFunction)
        {
            var sql = databaseFunction.Sql;
            if (string.IsNullOrEmpty(sql))
            {
                //without the sql, we can't do anything
                return "-- add function " + databaseFunction.Name;
            }
            if (sql.TrimStart().StartsWith("FUNCTION ", StringComparison.OrdinalIgnoreCase))
            {
                return "CREATE " + sql + _sqlFormatProvider.RunStatements();
            }
            //helpfully, SqlServer includes the create statement
            //MySQL doesn't, so this will need to be overridden
            return sql + _sqlFormatProvider.RunStatements();
        }

        public virtual string DropPackage(DatabasePackage databasePackage)
        {
            return null; //only applies to Oracle, so see it's override
        }

        public virtual string AddPackage(DatabasePackage databasePackage)
        {
            return null; //only applies to Oracle, so see it's override
        }

        public virtual string DropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "DROP INDEX {0}{1} ON {2};",
                SchemaPrefix(index.SchemaOwner),
                Escape(index.Name),
                TableName(databaseTable));
        }

        public virtual string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            return string.Format(CultureInfo.InvariantCulture,
                @"-- CREATE TRIGGER {0}{1} {2} ON {3};",
                SchemaPrefix(trigger.SchemaOwner),
                Escape(trigger.Name),
                trigger.TriggerEvent,
                TableName(databaseTable));
        }
        protected virtual string DropTriggerFormat
        {
            get { return "DROP TRIGGER {0}{1};"; }
        }
        public string DropTrigger(DatabaseTrigger trigger)
        {
            return string.Format(CultureInfo.InvariantCulture,
                DropTriggerFormat,
                SchemaPrefix(trigger.SchemaOwner),
                Escape(trigger.Name));
        }

        public string RunStatements()
        {
            return _sqlFormatProvider.RunStatements();
        }

        public string DropSequence(DatabaseSequence sequence)
        {
            return "DROP SEQUENCE " + SchemaPrefix(sequence.SchemaOwner) + Escape(sequence.Name) + ";";
        }

        public string AddSequence(DatabaseSequence sequence)
        {
            //amazingly SQLServer Denali has the same syntax as Oracle. http://msdn.microsoft.com/en-us/library/ff878091%28v=SQL.110%29.aspx
            var sb = new StringBuilder();
            sb.Append("CREATE SEQUENCE " + SchemaPrefix(sequence.SchemaOwner) + Escape(sequence.Name) +
                " INCREMENT BY " + sequence.IncrementBy);
            //min/max are optional- if they look like defaults, no need to write them out
            if (sequence.MinimumValue.HasValue && sequence.MinimumValue != 0)
            {
                sb.Append(" MINVALUE " + sequence.MinimumValue);
            }
            if (sequence.MaximumValue.HasValue && sequence.MaximumValue != 999999999999999999999999999M)
            {
                sb.Append(" MAXVALUE " + sequence.MaximumValue);
            }
            sb.Append(";");
            return sb.ToString();
        }

        public string AddIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            if (index.Columns.Count == 0)
            {
                //IndexColumns errors 
                return "-- add index " + index.Name + " (unknown columns)";
            }
            //we could plug in "CLUSTERED" or "PRIMARY XML" from index.IndexType here
            var indexType = index.IsUnique ? "UNIQUE " : string.Empty;

            return string.Format(CultureInfo.InvariantCulture,
                "CREATE {0}INDEX {1} ON {2}({3})",
                indexType, //must have trailing space
                Escape(index.Name),
                TableName(databaseTable),
                GetColumnList(index.Columns.Select(i => i.Name))) + LineEnding();
        }

        protected virtual string DropForeignKeyFormat
        {
            get { return "ALTER TABLE {0} DROP CONSTRAINT {1}"; }
        }
        protected virtual string DropUniqueFormat
        {
            get { return DropForeignKeyFormat; }
        }

        protected virtual bool SupportsDropColumn { get { return true; } }

        public virtual string DropColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
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
                        TableName(databaseTable),
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
                        TableName(databaseTable),
                        Escape(uniqueKey.Name)));
                }
            }
            sb.AppendLine("ALTER TABLE " + TableName(databaseTable) + " DROP COLUMN " + Escape(databaseColumn.Name) + LineEnding());
            return sb.ToString();
        }

        public virtual string DropDefault(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            return string.Format(CultureInfo.InvariantCulture,
                                 "ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT;",
                                 TableName(databaseTable),
                                 Escape(databaseColumn.Name));
        }

        /// <summary>
        /// Renames the table (if available)
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="originalTableName">Name of the original table.</param>
        /// <returns></returns>
        public virtual string RenameTable(DatabaseTable databaseTable, string originalTableName)
        {
            return "--TODO rename table " + Escape(originalTableName) + " to " + TableName(databaseTable);
        }

        protected string RenameTableTo(DatabaseTable databaseTable, string originalTableName)
        {
            if (string.IsNullOrEmpty(originalTableName) || databaseTable == null)
                return RenameTable(databaseTable, originalTableName);
            return string.Format(CultureInfo.InvariantCulture,
                                 "ALTER TABLE {0} RENAME TO {1};",
                                 SchemaPrefix(databaseTable.SchemaOwner) + Escape(originalTableName),
                                 Escape(databaseTable.Name));
        }

        public virtual string DropTable(DatabaseTable databaseTable)
        {
            var tableName = TableName(databaseTable);
            var sb = new StringBuilder();
            //drop foreign keys that refer to me
            foreach (var foreignKeyChild in databaseTable.ForeignKeyChildren)
            {
                foreach (var foreignKey in foreignKeyChild.ForeignKeys.Where(fk => fk.RefersToTable == databaseTable.Name))
                {
                    //table may have been dropped before, so check it exists
                    sb.AppendLine(IfConstraintExists(foreignKeyChild, foreignKey.Name));
                    sb.AppendLine(" ALTER TABLE " + TableName(foreignKeyChild) + " DROP CONSTRAINT " + Escape(foreignKey.Name) + ";");
                }
            }

            sb.AppendLine("DROP TABLE " + tableName + LineEnding());
            return sb.ToString();
        }

        private static string IfConstraintExists(DatabaseTable databaseTable, string foreignKeyName)
        {
            if (string.IsNullOrEmpty(databaseTable.SchemaOwner))
            {
                return string.Format(CultureInfo.InvariantCulture,
                    "IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}'))",
                    databaseTable.Name,
                    foreignKeyName);
            }
            return string.Format(CultureInfo.InvariantCulture,
                "IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}' AND CONSTRAINT_NAME = '{2}'))",
                databaseTable.SchemaOwner,
                databaseTable.Name,
                foreignKeyName);
        }

        /// <summary>
        /// Gets the escaped table name (prefixed with schema if present)
        /// </summary>
        protected string TableName(DatabaseTable databaseTable)
        {
            return SchemaPrefix(databaseTable.SchemaOwner) + Escape(databaseTable.Name);
        }

        /// <summary>
        /// If there is a schema (eg "dbo") returns it escaped with trailing dot ("[dbo].")
        /// </summary>
        protected string SchemaPrefix(string schema)
        {
            if (IncludeSchema && !string.IsNullOrEmpty(schema))
            {
                return Escape(schema) + ".";
            }
            return string.Empty;
        }

        private string GetColumnList(IEnumerable<string> columns)
        {
            var escapedColumnNames = columns.Select(column => Escape(column)).ToArray();
            return string.Join(", ", escapedColumnNames);
        }
    }
}
