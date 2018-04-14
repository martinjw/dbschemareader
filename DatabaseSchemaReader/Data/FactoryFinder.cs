using System.Data.Common;

namespace DatabaseSchemaReader.Data
{
    /// <summary>
    /// Finds the DbProviderFactory. May return null
    /// </summary>
    static class FactoryFinder
    {
        /// <summary>
        /// Finds the DbProviderFactory. May return null
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        public static DbProviderFactory FindFactory(DbConnection connection)
        {
            //since .net 2, there's been a protected virtual property on DbConnection for the factory
            //it's still in netstandard2, and it's still not public.
            var prop = connection.GetType().GetProperty("DbProviderFactory",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (DbProviderFactory)prop.GetValue(connection, null);
        }
    }
}
