using System.Text;
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

        public override string AddFunction(DatabaseFunction databaseFunction)
        {
            //the function.Sql contains the BEGIN to END statements, not the CREATE FUNCTION and arguments.
            //for now, just comment
            return "-- add function " + databaseFunction.Name;
        }

        public override string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            //mysql
            //CREATE TRIGGER (triggerName)
            //(BEFORE | AFTER) ([INSERT ] | [ UPDATE (OF Column) ] | [ DELETE ])
            //ON (tableName) 
            //FOR EACH ROW (sql_statement)

            /*
             * CREATE TRIGGER `ins_film` AFTER INSERT ON `film` FOR EACH ROW BEGIN
    INSERT INTO film_text (film_id, title, description)
        VALUES (new.film_id, new.title, new.description);
  END;;
             */


            var sb = new StringBuilder();
            sb.AppendLine("DELIMITER ;;");
            sb.AppendLine("CREATE TRIGGER "  +SchemaPrefix(trigger.SchemaOwner) + Escape(trigger.Name));
            sb.AppendLine(trigger.TriggerType + " " + trigger.TriggerEvent);
            sb.AppendLine("ON " + TableName(databaseTable));
            sb.AppendLine("FOR EACH ROW");
            sb.AppendLine(trigger.TriggerBody + ";;");
            sb.AppendLine("DELIMITER ;");
            return sb.ToString();
        }
    }
}
