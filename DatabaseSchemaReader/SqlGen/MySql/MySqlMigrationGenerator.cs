using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class MySqlMigrationGenerator : MigrationGenerator
    {
        public MySqlMigrationGenerator() : base(SqlType.MySql)
        {
        }

        protected override string DropForeignKeyFormat
        {
            get { return "ALTER TABLE {0} DROP FOREIGN KEY {1};"; }
        }
        protected override string DropUniqueFormat
        {
            get { return "ALTER TABLE {0} DROP INDEX {1};"; }
        }
        protected override string DropTriggerFormat
        {
            get { return "DROP IF EXISTS TRIGGER {0}{1};"; }
        }

        public override string AddProcedure(DatabaseStoredProcedure procedure)
        {
            //the procedure.Sql contains the BEGIN to END statements, not the CREATE PROCEDURE and arguments.
            //for now, just comment
            return "-- add procedure " + procedure.Name;
        }
    }
}
