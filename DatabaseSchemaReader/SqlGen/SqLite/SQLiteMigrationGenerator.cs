using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqLite
{
    class SqLiteMigrationGenerator : MigrationGenerator
    {
        public SqLiteMigrationGenerator()
            : base(SqlType.SQLite)
        {
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
    }
}
