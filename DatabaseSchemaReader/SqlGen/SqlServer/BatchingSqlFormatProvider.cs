namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    internal class BatchingSqlFormatProvider : ISqlFormatProvider
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

        public override string ToString()
        {
            return "SqlServer.BatchingSqlFormatProvider";
        }
    }
}