using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Db2
{
    class ProcedureGenerator : ProcedureGeneratorBase
    {
        public ProcedureGenerator(DatabaseTable table)
            : base(table)
        {
            SqlWriter = new SqlWriter(table, SqlType.Db2);
            SqlWriter.InStoredProcedure = true;
            SqlWriter.FormatParameter = x => { return "p_" + x; };
            FormatParameter = SqlWriter.FormatParameter;
        }
        protected override IProcedureWriter CreateProcedureWriter(string procName)
        {
            return new ProcedureWriter(procName, TableName, Table.SchemaOwner);
        }
        protected override string ColumnDataType(DatabaseColumn column)
        {
            return new DataTypeWriter().WriteDataType(column);
        }

        protected override string ColumnDataType(string dataType)
        {
            return dataType;
        }
    }
}
