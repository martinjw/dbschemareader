using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    /// Sort the tables in a database schema by foreign key dependency order
    /// </summary>
    public static class SchemaTablesSorter
    {
        /// <summary>
        /// Sorts the tables of the specified database schema.
        /// </summary>
        /// <param name="databaseSchema">The database schema.</param>
        public static void Sort(DatabaseSchema databaseSchema)
        {
            databaseSchema.Tables.Sort((a, b) =>
            {
                if (a == b) return 0; //the same
                if (a == null) return -1; //b is greater
                if (b == null) return 1; //a is greater

                //b depends on a so a is first
                if (b.ForeignKeyChildren.Contains(a)) return -1;
                return 1;
            });
        }
    }
}
