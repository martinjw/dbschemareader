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
            get { return "ALTER TABLE {0} ALTER COLUMN {1}"; }
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
                "DROP INDEX {0}.{1}",
                databaseTable.Name,
                Escape(index.Name));
        }

    }
}
