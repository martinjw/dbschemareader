using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    /// <summary>
    /// Generate MySql stored procedures.
    /// </summary>
    public class ProcedureGenerator : ProcedureGeneratorBase
    {
        public ProcedureGenerator(DatabaseTable table) : base(table)
        {
            SqlWriter = new SqlWriter(table, SqlType.MySql);
            SqlWriter.InStoredProcedure = true;
            SqlWriter.FormatParameter = x => { return "p_" + x; };
        }
        protected override IProcedureWriter CreateProcedureWriter(string procName)
        {
            return new ProcedureWriter(procName, TableName);
        }
        protected override string ColumnDataType(DatabaseColumn column)
        {
            return column.MySqlDataType();
        }

        protected override string ColumnDataType(string dataType)
        {
            return dataType;
        }
    }
}
