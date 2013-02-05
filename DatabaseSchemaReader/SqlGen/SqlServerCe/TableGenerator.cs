using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.SqlServer;

namespace DatabaseSchemaReader.SqlGen.SqlServerCe
{
    /// <summary>
    /// Table generator based on full SqlServer but using a derived DataTypeWriter for the subset of datatypes
    /// </summary>
    class TableGenerator : SqlServer.TableGenerator
    {
        public TableGenerator(DatabaseTable table)
            : base(table)
        {
            DataTypeWriter = new DataTypeWriter();
        }
        protected override ConstraintWriter CreateConstraintWriter()
        {
            var constraintWriter = base.CreateConstraintWriter();
            //check constraints don't seem to be included so ignore them all
            constraintWriter.CheckConstraintExcluder = check => true;
            return constraintWriter;
        }

        protected override IMigrationGenerator CreateMigrationGenerator()
        {
            //this will ensure the add constraints don't have schema naming
            return new SqlServerCeMigrationGenerator();
        }

        protected override bool HandleComputed(DatabaseColumn column)
        {
            return false; //computed columns aren't supported
        }
    }
}
