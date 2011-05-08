using DatabaseSchemaReader.DataSchema;

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
        //protected override ConstraintWriter CreateConstraintWriter()
        //{
        //    return new ConstraintWriter(Table) { IncludeSchema = IncludeSchema };
        //}
        protected override IMigrationGenerator CreateMigrationGenerator()
        {
            //this will ensure the add constraints don't have schema naming
            return new SqlServerCeMigrationGenerator();
        }
    }
}
