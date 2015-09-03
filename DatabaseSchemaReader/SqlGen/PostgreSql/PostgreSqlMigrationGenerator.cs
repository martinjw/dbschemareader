using System;
using System.Globalization;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.PostgreSql
{
    class PostgreSqlMigrationGenerator : MigrationGenerator
    {
        public PostgreSqlMigrationGenerator()
            : base(SqlType.PostgreSql)
        {
        }

        protected override string AlterColumnFormat
        {
            get { return "ALTER TABLE {0} ALTER COLUMN {1};"; }
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
        /// <param name="databaseTable">The database table.</param><param name="databaseColumn">The database column.</param><param name="originalColumn">The original column.</param>
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
            var dataType = dtw.WriteDataType(databaseColumn);

            var columnDef = Escape(databaseColumn.Name) + " TYPE " +
                            dataType.TrimEnd();
            return comment +
                Environment.NewLine +
                string.Format(CultureInfo.InvariantCulture,
                    AlterColumnFormat,
                    TableName(databaseTable),
                    columnDef.Replace("NOT NULL", String.Empty)) +
               string.Format("\r\nALTER TABLE {0} ALTER COLUMN {1} {2} NOT NULL;",
               TableName(databaseTable), Escape(databaseColumn.Name), (databaseColumn.Nullable ? "DROP" : "SET"));
        }
    }
}
