using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class ConstraintWriter : ConstraintWriterBase
    {
        public ConstraintWriter(DatabaseTable table) : base(table)
        {
        }

        #region Overrides of ConstraintWriterBase

        protected override string LineEnding()
        {
            return @";";
        }
        protected override string EscapeName(string name)
        {
            return StringEscaper.Escape(name);
        }

        #endregion
    }
}
