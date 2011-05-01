using System.Text;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;

namespace DatabaseSchemaReader.Compare
{
    /// <summary>
    /// Compares two schemas, and returns a migration script.
    /// </summary>
    public class CompareSchemas
    {
        private readonly ComparisonWriter _writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareSchemas"/> class.
        /// </summary>
        /// <param name="sqlType">Type of the SQL.</param>
        public CompareSchemas(SqlType sqlType)
        {
            _writer = new ComparisonWriter(sqlType);
        }

        /// <summary>
        /// Run the comparison.
        /// </summary>
        /// <param name="baseSchema">The base schema.</param>
        /// <param name="compareSchema">The compare schema.</param>
        /// <returns></returns>
        public string Execute(DatabaseSchema baseSchema, DatabaseSchema compareSchema)
        {
            var sb = new StringBuilder();

            var compareTables = new CompareTables(sb, _writer);
            //make sure they are in topological order- if 2 tables are added, the first must not have a foreign key to the second...
            var comparedTables = SchemaTablesSorter.TopologicalSort(compareSchema);
            compareTables.Execute(baseSchema.Tables, comparedTables);

            //compare views

            //compare stored procedures and functions

            //compare packages

            //compare sequences

            return sb.ToString();
        }
    }
}
