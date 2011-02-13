namespace DatabaseSchemaReader.SqlGen.Oracle
{
    static class StringEscaper
    {
        public static string Escape(string name)
        {
            return "\"" + name + "\"";
        }
    }
}
