namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    static class StringEscaper
    {
        public static string Escape(string name)
        {
            return "[" + name + "]";
        }
    }
}
