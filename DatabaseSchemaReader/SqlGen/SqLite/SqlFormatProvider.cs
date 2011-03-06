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
            return @";";
        }
    }
}
