using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.Oracle
{
    /// <summary>
    /// Generate Oracle stored procedures.
    /// </summary>
    internal class ProcedureGenerator : ProcedureGeneratorBase
    {
        private readonly StringBuilder _packageDefinition = new StringBuilder();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureGenerator"/> class.
        /// </summary>
        /// <param name="table">The table.</param>
        public ProcedureGenerator(DatabaseTable table)
            : base(table)
        {
            SqlWriter = new SqlWriter(table, SqlType.Oracle);
            SqlWriter.InStoredProcedure = true;
            PackageName = "PACK_" + TableName;
        }

        /// <summary>
        /// Gets or sets the name of the package. Defaults to PACK_tablename
        /// </summary>
        /// <value>The name of the package.</value>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the name of the cursor parameter. Defaults to Result.
        /// </summary>
        /// <value>The name of the cursor parameter.</value>
        public override string CursorParameterName { get; set; }

        protected override IProcedureWriter CreateProcedureWriter(string procName)
        {
            var writer = new ProcedureWriter(procName, TableName);
            if (!string.IsNullOrEmpty(CursorParameterName)) 
                writer.CursorParameterName = CursorParameterName;
            return writer;
        }

        protected override string ColumnDataType(DatabaseColumn column)
        {
            return DataTypeWriter.OracleDataTypeForParameter(column);
        }

        protected override string ColumnDataType(string dataType)
        {
            return DataTypeWriter.OracleDataType(dataType);
        }

        protected override void StartWrite()
        {
            _packageDefinition.AppendLine("CREATE OR REPLACE PACKAGE " + PackageName + " AS ");
            _packageDefinition.AppendLine("TYPE T_CURSOR IS REF CURSOR;");
            File.AppendLine("CREATE OR REPLACE PACKAGE BODY " + PackageName + " AS ");
            base.StartWrite();
        }
        protected override void WriteSignature(string signature)
        {
            _packageDefinition.AppendLine(signature);
        }
        protected override void Save()
        {
            _packageDefinition.AppendLine("END " + PackageName + ";");
            _packageDefinition.AppendLine("/");
            File.AppendLine("END " + PackageName + ";");
            File.AppendLine("/");
            File.Insert(0, _packageDefinition.ToString());
            base.Save();
        }
    }
}
