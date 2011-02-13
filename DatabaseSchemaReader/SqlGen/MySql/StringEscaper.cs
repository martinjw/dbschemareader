namespace DatabaseSchemaReader.SqlGen.MySql
{
    static class StringEscaper
    {
        public static string Escape(string name)
        {
            return "`" + name + "`";
        }
    }
}
