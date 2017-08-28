namespace DatabaseSchemaReader.SqlGen.Db2
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
            return ";";
        }

        public int MaximumNameLength => 30;
    }
}
