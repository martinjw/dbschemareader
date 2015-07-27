
namespace DatabaseSchemaReader.SqlGen.PostgreSql
{
    class SqlFormatProvider : ISqlFormatProvider
    {
        public string Escape(string name)
        {
            return "\"" + name + "\"";
        }

        public string LineEnding()
        {
            return ";";
        }

        public string RunStatements()
        {
            return string.Empty;
        }

        public int MaximumNameLength
        {
            get { return 63; }
        }
    }
}
