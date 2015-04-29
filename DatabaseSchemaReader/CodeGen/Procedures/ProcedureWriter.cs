using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen.Procedures
{
    /// <summary>
    /// Write a Stored Procedure ADO class.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="SprocWriter"/>
    /// </remarks>
    public class ProcedureWriter
    {
        private readonly DatabaseStoredProcedure _storedProcedure;
        private readonly string _namespace;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureWriter"/> class.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure.</param>
        /// <param name="ns">The namespace.</param>
        public ProcedureWriter(DatabaseStoredProcedure storedProcedure, string ns)
        {
            _namespace = ns;
            _storedProcedure = storedProcedure;
            
        }
        /// <summary>
        /// Writes this class.
        /// </summary>
        /// <returns></returns>
        public string Write()
        {
            var sw = new SprocWriter(_storedProcedure, _namespace);
            return sw.WriteWithResultClass();
        }
    }
}
