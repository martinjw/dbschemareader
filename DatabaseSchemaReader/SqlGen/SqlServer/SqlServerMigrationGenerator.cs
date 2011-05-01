using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class SqlServerMigrationGenerator : MigrationGenerator
    {
        public SqlServerMigrationGenerator() : base(SqlType.SqlServer)
        {
        }
        protected override string AlterColumnFormat
        {
            get { return "ALTER TABLE {0} ALTER COLUMN {1};"; }
        }
    }
}
