namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class SqlFormatProvider : ISqlFormatProvider
    {
        public string Escape(string name) => "[" + name + "]";

        public virtual string LineEnding() => ";";

        public string RunStatements() => "\r\nGO\r\n";

        public int MaximumNameLength => 128;
    }
}
