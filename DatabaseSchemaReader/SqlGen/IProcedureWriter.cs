namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Write a stored procedure
    /// </summary>
    interface IProcedureWriter
    {
        void AddOutputParameter(string parameterName, string dataType);
        void AddParameter(string parameterName, string dataType);
        void AddIntegerParameter(string parameterName);
        void AddSql(string sql);
        void AddQuerySql(string sql);
        void BeginProcedure();
        void BeginProcedure(bool hasQuery);
        string End();
        string Signature();
    }
}
