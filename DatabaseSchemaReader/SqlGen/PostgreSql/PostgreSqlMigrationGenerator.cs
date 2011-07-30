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
    }
}
