using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Override this to add custom code to the code editor
    /// </summary>
    public class CodeInserter
    {
        /// <summary>
        /// Write any namespaces.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public virtual string WriteNamespaces(DatabaseTable table)
        {
            return null;
        }

        /// <summary>
        /// Write any table annotations.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <returns></returns>
        public virtual string WriteTableAnnotations(DatabaseTable table)
        {
            return null;
        }

        /// <summary>
        /// Writes any column annotations.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public virtual string WriteColumnAnnotations(DatabaseTable table, DatabaseColumn column)
        {
            return null;
        }
    }
}
