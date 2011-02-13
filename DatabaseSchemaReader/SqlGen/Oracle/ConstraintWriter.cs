using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Oracle
{
    class ConstraintWriter : ConstraintWriterBase
    {
        public ConstraintWriter(DatabaseTable table)
            : base(table)
        {
        }

        protected override int MaximumNameLength
        {
            get { return 30; }
        }

        protected override string LineEnding()
        {
            return @";";
        }
        protected override string EscapeName(string name)
        {
            return StringEscaper.Escape(name);
        }
    }
}
