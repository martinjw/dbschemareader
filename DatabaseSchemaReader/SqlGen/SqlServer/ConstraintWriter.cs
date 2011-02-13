using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class ConstraintWriter : ConstraintWriterBase
    {
        public ConstraintWriter(DatabaseTable table)
            : base(table)
        {
        }

        protected override string LineEnding()
        {
            return @"
GO
";
        }
        protected override string EscapeName(string name)
        {
            return StringEscaper.Escape(name);
        }

    }
}
