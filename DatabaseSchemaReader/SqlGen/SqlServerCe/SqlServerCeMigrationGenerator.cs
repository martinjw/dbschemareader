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
    }
}
