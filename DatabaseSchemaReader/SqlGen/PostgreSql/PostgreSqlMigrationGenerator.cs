using System;
using System.Globalization;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.PostgreSql
{
    class PostgreSqlMigrationGenerator : MigrationGenerator
    {
        public PostgreSqlMigrationGenerator()
            : base(SqlType.PostgreSql)
        {
        }

        public override string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            //CREATE TRIGGER notify_dept AFTER INSERT OR UPDATE OR DELETE
            //ON DEPT
            //EXECUTE PROCEDURE note_dept();

            if (string.IsNullOrEmpty(trigger.TriggerBody))
                return "-- add trigger " + trigger.Name;

            return trigger.TriggerBody + ";";
        }

        public override string AddIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            var sql= base.AddIndex(databaseTable, index);

            if (!string.IsNullOrEmpty(sql) && !string.IsNullOrEmpty(index.Filter))
            {
                sql = $"{sql.Replace(LineEnding(),"")} WHERE {index.Filter};";
            }

            return sql;
        }

        public override string DropTable(DatabaseTable databaseTable)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "DROP TABLE IF EXISTS {0} CASCADE;",
                TableName(databaseTable));
        }

        public override string DropColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} DROP COLUMN {1} CASCADE;",
                TableName(databaseTable),
                Escape(databaseColumn.Name));
        }

        public override string DropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "DROP INDEX IF EXISTS {0} CASCADE;",
                Escape(index.Name));
        }

        protected override string DropTriggerFormat =>
            "DROP TRIGGER IF EXISTS {1} ON {0}{2};";

        public override string RenameColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, string originalColumnName)
        {
            return RenameColumnTo(databaseTable, databaseColumn, originalColumnName);
        }

        public override string RenameTable(DatabaseTable databaseTable, string originalTableName)
        {
            return RenameTableTo(databaseTable, originalTableName);
        }


        /// <summary>
        /// Alters the column.
        /// </summary>
        /// <param name="databaseTable">The database table.</param>
        /// <param name="databaseColumn">The database column.</param>
        /// <param name="originalColumn">The original column.</param>
        /// <returns/>
        public override string AlterColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, DatabaseColumn originalColumn)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            if (!AlterColumnIncludeDefaultValue)
            {
                tableGenerator.IncludeDefaultValues = false;
            }
            var columnDefinition = tableGenerator.WriteColumn(databaseColumn).Trim();
            var originalDefinition = "?";
            if (originalColumn != null)
            {
                originalDefinition = tableGenerator.WriteColumn(originalColumn).Trim();
            }

            //add a nice comment
            var comment = string.Format(CultureInfo.InvariantCulture,
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

            var dtw = new DataTypeWriter();
            var dataType = dtw.WriteDataType(databaseColumn)
                                //Not null must be done as separate statement
                                .Replace("NOT NULL", String.Empty)
                                .TrimEnd();

            var tableName = TableName(databaseTable);
            var columnName = Escape(databaseColumn.Name);

            //https://www.postgresql.org/docs/current/sql-altertable.html
            //defaults #135
            var setDefault = AlterColumnDefaultValue(databaseColumn, originalColumn, tableName, columnName);

            var sb = new StringBuilder();
            sb.AppendLine(comment);
            //#132 with USING CAST(name AS type)
            sb.AppendLine($"ALTER TABLE {tableName} ALTER COLUMN {columnName} TYPE {dataType} USING CAST({columnName} AS {dataType});");
            if (!string.IsNullOrEmpty(setDefault))
            {
                sb.AppendLine(setDefault);
            }
            sb.AppendLine($"ALTER TABLE {tableName} ALTER COLUMN {columnName} {(databaseColumn.Nullable ? "DROP" : "SET")} NOT NULL;");
            return sb.ToString();
        }

        private static string AlterColumnDefaultValue(DatabaseColumn databaseColumn, DatabaseColumn originalColumn,
            string tableName, string columnName)
        {
            //set or drop default will also be a separate alter statement (if required)
            var setDefault = string.Empty;
            //defaultValue may be empty string- maybe just check null here??
            if (originalColumn == null && databaseColumn.DefaultValue != null)
            {
                var defaultQuote = string.Empty;
                if (databaseColumn.DataType != null && databaseColumn.DataType.IsString) defaultQuote = "'";
                return $"ALTER TABLE {tableName} ALTER COLUMN {columnName} SET DEFAULT {defaultQuote}{databaseColumn.DefaultValue}{defaultQuote};";
            }

            if (originalColumn != null && originalColumn.DefaultValue != databaseColumn.DefaultValue)
            {
                //changed default
                if (databaseColumn.DefaultValue == null)
                {
                    setDefault = $"ALTER TABLE {tableName} ALTER COLUMN {columnName} DROP DEFAULT;";
                }
                else
                {
                    var defaultQuote = string.Empty;
                    if (databaseColumn.DataType != null && databaseColumn.DataType.IsString) defaultQuote = "'";
                    setDefault = $"ALTER TABLE {tableName} ALTER COLUMN {columnName} SET DEFAULT {defaultQuote}{databaseColumn.DefaultValue}{defaultQuote};";
                }
            }

            return setDefault;
        }
    }
}
