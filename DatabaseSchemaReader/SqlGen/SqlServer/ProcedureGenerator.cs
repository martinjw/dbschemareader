using DatabaseSchemaReader.DataSchema;
using Library.Data.SqlGen;
using Library.Data.SqlGen.SqlServer;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    /// <summary>
    /// Generate SqlServer stored procedures.
    /// </summary>
    public class ProcedureGenerator : ProcedureGeneratorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureGenerator"/> class.
        /// </summary>
        /// <param name="table">The table.</param>
        public ProcedureGenerator(DatabaseTable table)
            : base(table)
        {
            SqlWriter = new SqlWriter(table, SqlType.SqlServer);
        }

        protected override IProcedureWriter CreateProcedureWriter(string procName)
        {
            return new ProcedureWriter(procName, TableName, Table.SchemaOwner);
        }
        protected override string ColumnDataType(DatabaseColumn column)
        {
            return column.SqlServerDataType();
        }
        protected override string ColumnDataType(string dataType)
        {
            return DataTypeWriter.SqlServerDataType(dataType);
        }
    }
}
