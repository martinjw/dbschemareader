namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Generate a table DDL
    /// </summary>
    public interface ITableGenerator
    {
        void WriteToFolder(string path);
    }
}
