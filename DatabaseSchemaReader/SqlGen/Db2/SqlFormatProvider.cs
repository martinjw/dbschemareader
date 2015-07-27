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

        public int MaximumNameLength
        {
            get { return 30; } //just 18 on OS/390 and AS/400 
        }
    }
}
