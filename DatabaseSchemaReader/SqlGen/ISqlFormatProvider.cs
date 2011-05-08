namespace DatabaseSchemaReader.SqlGen
{
    interface ISqlFormatProvider
    {
        string Escape(string name);
        string LineEnding();
        string RunStatements();
    }
}
