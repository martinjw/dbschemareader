using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServerCe
{
    class ConstraintWriter : SqlServer.ConstraintWriter
    {
        public ConstraintWriter(DatabaseTable table)
            : base(table)
        {
        }

        protected override ISqlFormatProvider SqlFormatProvider()
        {
            return new SqlServerCeFormatProvider();
        }
    }
}
