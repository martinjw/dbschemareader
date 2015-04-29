using System;
using System.Globalization;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Oracle
{
    class OracleMigrationGenerator : MigrationGenerator
    {
        public OracleMigrationGenerator()
            : base(SqlType.Oracle)
        {
        }

        public override string DropTable(DatabaseTable databaseTable)
        {
            return "DROP TABLE " + TableName(databaseTable) + " CASCADE CONSTRAINTS;";
        }
        public override string DropPackage(DatabasePackage databasePackage)
        {
            return "DROP PACKAGE " + SchemaPrefix(databasePackage.SchemaOwner) + Escape(databasePackage.Name) + ";";
        }

        public override string AddPackage(DatabasePackage databasePackage)
        {
            if (string.IsNullOrEmpty(databasePackage.Definition) || string.IsNullOrEmpty(databasePackage.Body))
            {
                return "-- add package " + databasePackage.Name + " (no sql available)";
            }
            //the body and defintion starts "PACKAGE name AS", so just add CREATE OR REPLACE
            return string.Format(CultureInfo.InvariantCulture,
                @"CREATE OR REPLACE  
{0}
/

CREATE OR REPLACE 
{1}
/
",
                databasePackage.Definition,
                databasePackage.Body);
        }
        public override string AddTrigger(DatabaseTable databaseTable, DatabaseTrigger trigger)
        {
            //oracle: 
            //CREATE (OR REPLACE) TRIGGER (triggerName) 
            //(BEFORE | AFTER | INSTEAD OF) ([INSERT ] [ OR ] [ UPDATE ] [ OR ] [ DELETE ])
            //ON (tableName) 
            //(FOR EACH ROW)
            //(sql_statement); /
            var beforeOrAfter = trigger.TriggerType;
            var forEachRow = string.Empty;
            if (beforeOrAfter.EndsWith(" EACH ROW", StringComparison.OrdinalIgnoreCase))
            {
                //it's not table level
                forEachRow = "FOR EACH ROW";
                beforeOrAfter = beforeOrAfter.Replace(" EACH ROW", "");
                //hopefully beforeOrAfter says "BEFORE", "AFTER" or "INSTEAD OF" now
            }
            else
            {
                beforeOrAfter = beforeOrAfter.Replace(" STATEMENT", "");
            }
            return string.Format(CultureInfo.InvariantCulture,
                @"CREATE OR REPLACE TRIGGER {0}{1}
{2} {3}
ON {4}
{5}
{6}
/
",
                SchemaPrefix(trigger.SchemaOwner),
                Escape(trigger.Name),
                beforeOrAfter,
                trigger.TriggerEvent,
                TableName(databaseTable),
                forEachRow,
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
