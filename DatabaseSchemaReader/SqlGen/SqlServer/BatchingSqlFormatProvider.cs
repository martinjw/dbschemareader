namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class BatchingSqlFormatProvider : ISqlFormatProvider
    {
        public string Escape(string name)
        {
            return "[" + name + "]";
        }

        public virtual string LineEnding()
        {
            return RunStatements();
        }


        public string RunStatements()
        {
            return @"
GO
"; ;
        }

        public int MaximumNameLength
        {
            get { return 128; }
        }
    }
}