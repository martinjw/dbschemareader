namespace DatabaseSchemaReader.SqlGen.SqLite
{
    class SqlFormatProvider : ISqlFormatProvider
    {
        public string Escape(string name)
        {
            return "[" + name + "]";
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
            get { return 256; } //there is no hard limit in SQLite
        }
    }
}
