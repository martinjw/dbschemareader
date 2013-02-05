using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    /// <summary>
    /// Generate SqlServer stored procedures.
    /// </summary>
    internal class ProcedureGenerator : ProcedureGeneratorBase
    {
        private readonly DataTypeWriter _dataTypeWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureGenerator"/> class.
        /// </summary>
        /// <param name="table">The table.</param>
        public ProcedureGenerator(DatabaseTable table)
            : base(table)
        {
            SqlWriter = new SqlWriter(table, SqlType.SqlServer);
            SqlType? originSqlType = null;
            if (table.DatabaseSchema != null)
                originSqlType = ProviderToSqlType.Convert(table.DatabaseSchema.Provider);
            _dataTypeWriter = new DataTypeWriter(originSqlType);
        }

        protected override IProcedureWriter CreateProcedureWriter(string procName)
        {
            return new ProcedureWriter(procName, TableName, Table.SchemaOwner);
        }
        protected override string ColumnDataType(DatabaseColumn column)
        {
            return _dataTypeWriter.WriteDataType(column);
        }
        protected override string ColumnDataType(string dataType)
        {
            return DataTypeWriter.WriteDataType(dataType);
        }
    }
}
