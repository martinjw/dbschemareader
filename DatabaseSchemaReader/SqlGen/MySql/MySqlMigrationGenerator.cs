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
    }
}
