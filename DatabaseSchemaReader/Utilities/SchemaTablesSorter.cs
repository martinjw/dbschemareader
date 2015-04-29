using System;
using System.Collections.Generic;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    /// Sort the tables in a database schema by foreign key dependency order.
    /// </summary>
    public static class SchemaTablesSorter
    {
        /// <summary>
        /// Sorts tables by the foreign key dependencies.
        /// </summary>
        /// <param name="databaseSchema">The database schema.</param>
        /// <returns></returns>
        public static IEnumerable<DatabaseTable> TopologicalSort(DatabaseSchema databaseSchema)
        {
            var tables = databaseSchema.Tables;
            var sorter = new TopologicalSorter(tables.Count);

            var indexes = new Dictionary<string, int>();

            //add vertices  
            for (var i = 0; i < tables.Count; i++)
            {
                indexes[tables[i].Name] = sorter.AddVertex(i);
            }

            //add edges  
            for (var i = 0; i < tables.Count; i++)
            {
                foreach (var t in tables[i].ForeignKeys)
                {
                    //ignore cycles (not relevant here)
                    if (t.RefersToTable == tables[i].Name) continue;
                    //corrupt model
                    if (!indexes.ContainsKey(t.RefersToTable)) continue;

                    sorter.AddEdge(i, indexes[t.RefersToTable]);
                }
            }

            try
            {
                var sortedIndex = sorter.Sort();
                return sortedIndex.Reverse().Select(i => tables[i]);
            }
            catch (InvalidOperationException)
            {
                //Bidirectional foreign keys create a cyclic dependency
                //Topological sort won't work
                //There is no way to sort by insert order for two such tables
                //Just give them the tables back, with those with least foreign keys at the start.
                return tables.OrderBy(t => t.ForeignKeys.Count);
            }
        }
    }
}
