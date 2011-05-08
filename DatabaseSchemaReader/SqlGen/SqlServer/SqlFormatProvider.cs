namespace DatabaseSchemaReader.SqlGen.SqlServer
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
            return @"
GO
"; ;
        }
    }
}
