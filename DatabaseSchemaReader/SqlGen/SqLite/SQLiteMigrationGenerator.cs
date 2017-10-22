using System.Globalization;
using DatabaseSchemaReader.DataSchema;
using System.Text;
using System;
using DatabaseSchemaReader.Utilities;
using System.Linq;

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

        public override string AddIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            if (index.Name.StartsWith("sqlite_autoindex_"))
            {
                return "--skipping reserved index: " + index.Name;
            }
            return base.AddIndex(databaseTable, index);
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

        public override string BackupAndUpdateTable(DatabaseTable databaseTable, DatabaseTable newTable)
        {
            StringBuilder sb = new StringBuilder();
            var gen = new TableGenerator(newTable);
            sb.AppendLine(gen.Write());
            string newColumns = newTable.GetFormattedColumnList(SqlType.SQLite);
            string selectColumns = newColumns;
            var ndiff = newTable.GetColumnList().Except(databaseTable.GetColumnList());
            if (ndiff.Count() == 1)
            {
                //if column names don't match, must be a rename
                //so we get the old column name that is different
                //and use it to replace new column name in select
                var diff = databaseTable.GetColumnList().Except(newTable.GetColumnList());
                selectColumns = newColumns.Replace(ndiff.First(), diff.First());
            }
            sb.AppendFormat("INSERT INTO {0} ({4}) SELECT {1} FROM {2};{3}", newTable.Name,
                        selectColumns, databaseTable.Name, Environment.NewLine, newColumns);
            sb.AppendFormat("DROP TABLE {0};{1}", databaseTable.Name, Environment.NewLine);
            sb.AppendFormat("ALTER TABLE {0} RENAME TO {1};{2}", newTable.Name, databaseTable.Name, Environment.NewLine);
            return sb.ToString();
        }

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
            if (originalDefinition.Equals(columnDefinition))
            {
                //most likely faulty sql db structure, skip
                //add a nice comment
                return string.Format(CultureInfo.InvariantCulture,
                    "-- skipping alter of {0} from {1} to {2}",
                    databaseTable.Name,
                    originalDefinition,
                    columnDefinition);
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("--TO CHANGE COLUMN {0} to {1}, WE CREATE BACKUP, MOVE CONTENT AND RENAME TABLE " + LineEnding(), originalDefinition, columnDefinition));

            DatabaseTable tempTable = databaseTable.Clone("bkup1903_" + databaseTable.Name);
            tempTable.Columns.Remove(tempTable.FindColumn(originalColumn == null ? databaseColumn.Name : originalColumn.Name));
            tempTable.Columns.Add(databaseColumn);
            sb.Append(BackupAndUpdateTable(databaseTable, tempTable));
            return sb.ToString();
        }
    }
}
