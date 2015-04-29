namespace DatabaseSchemaReader.SqlGen.MySql
{
    class SqlFormatProvider : ISqlFormatProvider
    {
        public string Escape(string name)
        {
            return "`" + name + "`";
        }

        public string LineEnding()
        {
            return ";";
        }

        public string RunStatements()
        {
            return ";";
        }

        public int MaximumNameLength
        {
            get { return 64; }
        }
    }
}
