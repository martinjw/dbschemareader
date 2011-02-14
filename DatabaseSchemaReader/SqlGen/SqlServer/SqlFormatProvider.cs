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
            return @"
GO
";
        }
    }
}
