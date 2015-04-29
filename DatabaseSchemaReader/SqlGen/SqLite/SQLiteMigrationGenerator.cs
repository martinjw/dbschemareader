using System.Globalization;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqLite
{
    class SqLiteMigrationGenerator : MigrationGenerator
    {
        public SqLiteMigrationGenerator()
            : base(SqlType.SQLite)
        {
        }

        public override string DropTable(DatabaseTable databaseTable)
        {
            //automatically removes fk references
            return "DROP TABLE " + databaseTable.Name + ";";
        }

        protected override bool SupportsAlterColumn { get { return false; } }
        protected override bool SupportsDropColumn { get { return false; } }

        public override string AddConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            return null; //doesn't support it
        }
        public override string DropConstraint(DatabaseTable databaseTable, DatabaseConstraint constraint)
        {
            return null; //doesn't support it
        }
        public override string AddFunction(DatabaseFunction databaseFunction)
        {
            return null; //doesn't support it
        }
        public override string AddProcedure(DatabaseStoredProcedure procedure)
        {
            return null; //doesn't support it
        }
        public override string DropFunction(DatabaseFunction databaseFunction)
        {
            return null; //doesn't support it
        }
        public override string DropProcedure(DatabaseStoredProcedure procedure)
        {
            return null; //doesn't support it
        }

        protected override string DropTriggerFormat
        {
            get { return "DROP IF EXISTS TRIGGER {1};"; }
        }
        public override string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            //sqlite: 
            //CREATE TRIGGER (triggerName) (IF NOT EXISTS)
            //(BEFORE | AFTER | INSTEAD OF) ([INSERT ] | [ UPDATE (OF Column) ] | [ DELETE ])
            //ON (tableName) 
            //(FOR EACH ROW)
            //BEGIN (sql_statement); END

            return string.Format(CultureInfo.InvariantCulture,
                @"CREATE TRIGGER {0} IF NOT EXISTS
{1} {2}
ON {3}
{4};",
                Escape(trigger.Name),
                trigger.TriggerType,
                trigger.TriggerEvent,
                TableName(databaseTable),
                trigger.TriggerBody);
        }

        public override string DropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            //no "ON table" syntax
            return string.Format(CultureInfo.InvariantCulture,
                "DROP INDEX {0}{1};",
                SchemaPrefix(index.SchemaOwner),
                Escape(index.Name));
        }

        public override string DropDefault(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            return "-- drop default on " + databaseColumn.Name;
        }

        public override string RenameTable(DatabaseTable databaseTable, string originalTableName)
        {
            return RenameTableTo(databaseTable, originalTableName);
        }
    }
}
