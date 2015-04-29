using System;
using System.Globalization;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class MySqlMigrationGenerator : MigrationGenerator
    {
        public MySqlMigrationGenerator()
            : base(SqlType.MySql)
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
            if (string.IsNullOrEmpty(procedure.Sql))
            {
                //the procedure.Sql contains the BEGIN to END statements, not the CREATE PROCEDURE and arguments.
                //for now, just comment
                return "-- add procedure " + procedure.Name;
            }

            var name = procedure.Name;
            var procWriter = new ProcedureWriter(name, null);
            WriteProcedure(procedure, procWriter);

            return procWriter.End();
        }

        private static void WriteProcedure(DatabaseStoredProcedure procedure, ProcedureWriter procWriter)
        {
            foreach (var argument in procedure.Arguments)
            {
                if (argument.Out)
                {
                    //we don't deal with INOUT parameters.
                    procWriter.AddOutputParameter(argument.Name, argument.DatabaseDataType);
                    continue;
                }
                //an IN sproc
                procWriter.AddParameter(argument.Name, argument.DatabaseDataType);
            }
            procWriter.BeginProcedure();

            var sql = procedure.Sql.Trim()
                //standardize to windows line endings
                .Replace("\r\n","\n").Replace("\r","\n").Replace("\n","\r\n");
            //remove the BEGIN and END as the procWriter writes these
            if (sql.StartsWith("BEGIN", StringComparison.OrdinalIgnoreCase))
            {
                sql = sql.Substring(5);
            }
            if (sql.EndsWith("END", StringComparison.OrdinalIgnoreCase))
            {
                sql = sql.Substring(0, sql.Length - 3).Trim();
            }

            procWriter.AddSql(sql);
        }

        public override string AddFunction(DatabaseFunction databaseFunction)
        {
            if (string.IsNullOrEmpty(databaseFunction.Sql))
            {
                //the function.Sql contains the BEGIN to END statements, not the CREATE FUNCTION and arguments.
                //for now, just comment
                return "-- add function " + databaseFunction.Name;
            }

            var name = databaseFunction.Name;
            var procWriter = new ProcedureWriter(name, true);
            procWriter.AddReturns(databaseFunction.ReturnType);
            WriteProcedure(databaseFunction, procWriter);

            return procWriter.End();

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
            sb.AppendLine("CREATE TRIGGER " + SchemaPrefix(trigger.SchemaOwner) + Escape(trigger.Name));
            sb.AppendLine(trigger.TriggerType + " " + trigger.TriggerEvent);
            sb.AppendLine("ON " + TableName(databaseTable));
            sb.AppendLine("FOR EACH ROW");
            sb.AppendLine(trigger.TriggerBody + ";;");
            sb.AppendLine("DELIMITER ;");
            return sb.ToString();
        }

        public override string RenameColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn, string originalColumnName)
        {
            if (string.IsNullOrEmpty(originalColumnName) || databaseColumn == null)
                return RenameColumn(databaseTable, databaseColumn, originalColumnName);
            //MySql has to restate the column definition even if it's unchanged. Yuck, but we have the data.
            var tableGenerator = CreateTableGenerator(databaseTable);
            var columnDefinition = tableGenerator.WriteColumn(databaseColumn).Trim();
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} CHANGE {1} {2}",
                TableName(databaseTable),
                Escape(originalColumnName),
                columnDefinition) + LineEnding();
        }

        public override string RenameTable(DatabaseTable databaseTable, string originalTableName)
        {
            if (string.IsNullOrEmpty(originalTableName) || databaseTable == null)
                return RenameTable(databaseTable, originalTableName);
            return string.Format(CultureInfo.InvariantCulture,
                                 "RENAME TABLE {0} TO {1};",
                                 SchemaPrefix(databaseTable.SchemaOwner) + Escape(originalTableName),
                                 Escape(databaseTable.Name));
        }
    }
}
