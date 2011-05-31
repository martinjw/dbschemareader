using System.Globalization;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Db2
{
    class Db2MigrationGenerator : MigrationGenerator
    {
        public Db2MigrationGenerator()
            : base(SqlType.Db2)
        {
        }

        protected override string AlterColumnFormat
        {
            get { return "ALTER TABLE {0} ALTER COLUMN {1};"; }
        }

        public override string AddColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            var addColumn = tableGenerator.WriteColumn(databaseColumn).Trim();
            if (string.IsNullOrEmpty(databaseColumn.DefaultValue) && !databaseColumn.Nullable)
            {
                //for strings, a zero length string; for numbers, 0; for dates, current timestamp
                addColumn += " DEFAULT";
            }
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} ADD {1}",
                TableName(databaseTable),
                addColumn) + LineEnding();
        }

        public override string DropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "DROP INDEX {0}{1};",
                SchemaPrefix(index.SchemaOwner),
                Escape(index.Name));
        }

        public override string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            if (string.IsNullOrEmpty(trigger.TriggerBody))
                return "-- add trigger " + trigger.Name;

            return trigger.TriggerBody + ";";
        }
    }
}
