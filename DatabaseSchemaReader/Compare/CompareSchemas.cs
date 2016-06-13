using System.Collections.Generic;
using System.Text;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities;

namespace DatabaseSchemaReader.Compare
{
    /// <summary>
    /// Compares two schemas, and returns a migration script.
    /// </summary>
    /// <remarks>
    /// Take care with sorting. 
    /// Tables are sorted with those without foreign keys first.
    /// We don't know the dependency order for views and procedures, so a view that depends on another view may be written first and the script will fail.
    /// </remarks>
    public class CompareSchemas
    {
        private readonly ComparisonWriter _writer;
        private readonly DatabaseSchema _baseSchema;
        private readonly DatabaseSchema _compareSchema;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareSchemas"/> class.
        /// </summary>
        /// <param name="baseSchema">The base schema.</param>
        /// <param name="compareSchema">The compare schema.</param>
        public CompareSchemas(DatabaseSchema baseSchema, DatabaseSchema compareSchema)
        {
            //argument null
            if (baseSchema == null) baseSchema = new DatabaseSchema(null, null);
            if (compareSchema == null) compareSchema = new DatabaseSchema(null, null);

            _compareSchema = compareSchema;
            _baseSchema = baseSchema;

            SqlType sqlType = FindSqlType(compareSchema) ?? FindSqlType(baseSchema) ?? SqlType.SqlServer;

            _writer = new ComparisonWriter(sqlType);
        }


        private static SqlType? FindSqlType(DatabaseSchema databaseSchema)
        {
            var providerName = databaseSchema.Provider;
            return ProviderToSqlType.Convert(providerName);
        }
        /// <summary>
        /// Run the comparison.
        /// </summary>
        /// <returns></returns>
        public string Execute()
        {
            var sb = new StringBuilder();

            var results = ExecuteResult();
            foreach (var compareResult in results)
            {
                sb.AppendLine(compareResult.Script);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Run the comparison and returns a <see cref="CompareResult"/>.
        /// </summary>
        /// <returns></returns>
        public IList<CompareResult> ExecuteResult()
        {
            var list = new List<CompareResult>();

            var compareTables = new CompareTables(list, _writer);
            //make sure they are in topological order- if 2 tables are added, the first must not have a foreign key to the second...
            var comparedTables = SchemaTablesSorter.TopologicalSort(_compareSchema);
            compareTables.Execute(_baseSchema.Tables, comparedTables);

            //compare sequences
            var compareSequences = new CompareSequences(list, _writer);
            compareSequences.Execute(_baseSchema.Sequences, _compareSchema.Sequences);

            //compare views
            var compareViews = new CompareViews(list, _writer);
            compareViews.Execute(_baseSchema.Views, _compareSchema.Views);

            //compare stored procedures and functions
            var compareProcedures = new CompareProcedures(list, _writer);
            compareProcedures.Execute(_baseSchema.StoredProcedures, _compareSchema.StoredProcedures);
            var compareFunctions = new CompareFunctions(list, _writer);
            compareFunctions.Execute(_baseSchema.Functions, _compareSchema.Functions);

            //compare packages
            var comparePackages = new ComparePackages(list, _writer);
            comparePackages.Execute(_baseSchema.Packages, _compareSchema.Packages);

            return list;
        }
    }
}
