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
        private readonly bool _noSchema; //database has no schema, so don't use it

        public MigrationGenerator(SqlType sqlType)
        {
            _sqlFormatProvider = SqlFormatFactory.Provider(sqlType);
            _ddlFactory = new DdlGeneratorFactory(sqlType);
            _noSchema = (sqlType == SqlType.SqlServerCe || sqlType == SqlType.SQLite);
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
        protected virtual string LineEnding()
        {
            return SqlFormatProvider().LineEnding();
        }
        public string AddTable(DatabaseTable databaseTable)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            return tableGenerator.Write().Trim();
        }

        public string AddColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} ADD {1}",
                TableName(databaseTable),
                tableGenerator.WriteColumn(databaseColumn).Trim()) + LineEnding();
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
                    TableName(databaseTable),
                    columnDefinition);
        }


        public virtual string AddConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            //we always use the named form.
            var constraintName = constraint.Name;

            if (string.IsNullOrEmpty(constraintName)) throw new InvalidOperationException("Constraint must have a name");
            if (constraint.Columns.Count == 0) throw new InvalidOperationException("Constraint has no columns");

            //use the standard constraint writer for the database
            var constraintWriter = _ddlFactory.ConstraintWriter(databaseTable);
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
            return "DROP VIEW " + SchemaPrefix(view.SchemaOwner) + Escape(view.Name) + ";";
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

        public string DropProcedure(DatabaseStoredProcedure procedure)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "DROP PROCEDURE {0}{1};",
                SchemaPrefix(procedure.SchemaOwner),
                Escape(procedure.Name));
        }

        public string DropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "DROP INDEX {0}{1} ON {2};",
                SchemaPrefix(index.SchemaOwner),
                Escape(index.Name),
                TableName(databaseTable));
        }

        public string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            //sqlserver: 
            //CREATE TRIGGER (triggerName) 
            //ON (tableName) 
            //(FOR | AFTER | INSTEAD OF) ( [INSERT ] [ , ] [ UPDATE ] [ , ] [ DELETE ])
            //AS (sql_statement); GO 

            //oracle: 
            //CREATE (OR REPLACE) TRIGGER (triggerName) 
            //(BEFORE | AFTER | INSTEAD OF) ([INSERT ] [ OR ] [ UPDATE ] [ OR ] [ DELETE ])
            //ON (tableName) 
            //(FOR EACH ROW)
            //(sql_statement); /

            //sqlite: 
            //CREATE TRIGGER (triggerName) (IF NOT EXITSTS)
            //(BEFORE | AFTER | INSTEAD OF) ([INSERT ] | [ UPDATE (OF Column) ] | [ DELETE ])
            //ON (tableName) 
            //(FOR EACH ROW)
            //BEGIN (sql_statement); END

            //mysql
            //CREATE TRIGGER (triggerName)
            //(BEFORE | AFTER) ([INSERT ] | [ UPDATE (OF Column) ] | [ DELETE ])
            //ON (tableName) 
            //FOR EACH ROW (sql_statement)

            return string.Format(CultureInfo.InvariantCulture,
                @"CREATE TRIGGER {0}{1} {2} ON {3} 
BEGIN 
{4}
END;",
                SchemaPrefix(trigger.SchemaOwner),
                Escape(trigger.Name),
                trigger.TriggerEvent,
                TableName(databaseTable),
                trigger.TriggerBody);
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

        public string AddIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "CREATE INDEX {0} ON {1}({2})",
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
                    sb.AppendLine(IfTableExists(foreignKeyChild));
                    sb.AppendLine(" ALTER TABLE " + TableName(foreignKeyChild) + " DROP CONSTRAINT " + Escape(foreignKey.Name) + ";");
                }
            }

            sb.AppendLine("DROP TABLE " + tableName + LineEnding());
            return sb.ToString();
        }

        private static string IfTableExists(DatabaseTable databaseTable)
        {
            if (string.IsNullOrEmpty(databaseTable.SchemaOwner))
            {
                return "IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + databaseTable.Name + "'))";
            }
            return "IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = '" +
                   databaseTable.SchemaOwner + "' AND TABLE_NAME = '" + databaseTable.Name + "'))";
        }

        /// <summary>
        /// Gets the escaped table name (prefixed with schema if present)
        /// </summary>
        private string TableName(DatabaseTable databaseTable)
        {
            return SchemaPrefix(databaseTable.SchemaOwner) + Escape(databaseTable.Name);
        }

        private string SchemaPrefix(string schema)
        {
            if (!_noSchema && !string.IsNullOrEmpty(schema))
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
