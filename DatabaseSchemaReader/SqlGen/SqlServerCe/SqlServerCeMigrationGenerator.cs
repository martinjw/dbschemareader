using System.Globalization;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServerCe
{
    class SqlServerCeMigrationGenerator : MigrationGenerator
    {
        public SqlServerCeMigrationGenerator()
            : base(SqlType.SqlServerCe)
        {
        }
        protected override string LineEnding()
        {
            return SqlFormatProvider().RunStatements();
        }
        protected override string AlterColumnFormat
        {
            get { return "ALTER TABLE {0} ALTER COLUMN {1};"; }
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

        public override string DropIndex(DatabaseTable databaseTable, DatabaseIndex index)
        {
            //no "ON table" syntax
            return string.Format(CultureInfo.InvariantCulture,
                "DROP INDEX {0}.{1};",
                databaseTable.Name,
                Escape(index.Name));
        }

        public override string AddColumn(DatabaseTable databaseTable, DatabaseColumn databaseColumn)
        {
            var tableGenerator = CreateTableGenerator(databaseTable);
            var addColumn = tableGenerator.WriteColumn(databaseColumn).Trim();
            if (string.IsNullOrEmpty(databaseColumn.DefaultValue) && !databaseColumn.Nullable)
            {
                var dt = databaseColumn.DataType;
                if (dt == null || dt.IsString)
                {
                    addColumn += " DEFAULT '1'";
                }
                else if (dt.IsNumeric)
                {
                    addColumn += " DEFAULT 0";
                }
                else if (dt.IsDateTime)
                {
                    addColumn += " DEFAULT CURRENT_TIMESTAMP";
                }
            }
            return string.Format(CultureInfo.InvariantCulture,
                "ALTER TABLE {0} ADD {1}",
                TableName(databaseTable),
                addColumn) + LineEnding();
        }

    }
}
