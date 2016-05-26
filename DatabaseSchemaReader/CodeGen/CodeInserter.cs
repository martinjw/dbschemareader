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
        /// <param name="classBuilder">The class builder.</param>
        public virtual void WriteNamespaces(DatabaseTable table, ClassBuilder classBuilder)
        {
            //
        }

        /// <summary>
        /// Write any table annotations.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="classBuilder">The class builder.</param>
        public virtual void WriteTableAnnotations(DatabaseTable table, ClassBuilder classBuilder)
        {
            //
        }

        /// <summary>
        /// Writes any column annotations.
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="column">The column.</param>
        /// <param name="classBuilder">The class builder.</param>
        public virtual void WriteColumnAnnotations(DatabaseTable table, DatabaseColumn column, ClassBuilder classBuilder)
        {
            //
        }

        /// <summary>
        /// Writes any class members (custom fields, properties, methods).
        /// </summary>
        /// <param name="table">The table.</param>
        /// <param name="cb">The cb.</param>
        public virtual void WriteClassMembers(DatabaseTable table, ClassBuilder cb)
        {
            //
        }
    }
}
