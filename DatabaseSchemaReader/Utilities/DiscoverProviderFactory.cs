using System;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    /// A simple tool to discover what an ADO provider GetSchema provides
    /// </summary>
    public static class DiscoverProviderFactory
    {
        /// <summary>
        /// Discovers the specified connection string. NO ERROR TRAPPING.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">Name of the provider.</param>
        public static void Discover(string connectionString, string providerName)
        {

            var factory = System.Data.Common.DbProviderFactories.GetFactory(providerName);
            using (var connection = factory.CreateConnection())
            {
                connection.ConnectionString = connectionString;
                connection.Open();
                string metaDataCollections = System.Data.Common.DbMetaDataCollectionNames.MetaDataCollections;
                var dt = connection.GetSchema(metaDataCollections);
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    var collectionName = (string)row["CollectionName"];
                    Console.WriteLine(collectionName);
                    if (collectionName != metaDataCollections)
                    {
                        try
                        {
                            var col = connection.GetSchema(collectionName);
                            foreach (System.Data.DataColumn column in col.Columns)
                            {
                                Console.WriteLine("\t" + column.ColumnName + "\t" + column.DataType.Name);
                            }
                        }
                        catch (NotImplementedException)
                        {
                            
                                Console.WriteLine("\t" + collectionName + " not implemented");
                        }
                    }
                }
                connection.Close();
            }
        }
    }
}
