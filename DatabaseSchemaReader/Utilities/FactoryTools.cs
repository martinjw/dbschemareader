using System;
using System.Data;
using System.Data.Common;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    /// Tools to help with DbProviderFactory
    /// </summary>
    public static class FactoryTools
    {
        private static DbProviderFactory _manualProviderFactory;

        /// <summary>
        /// Finds the factory.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        public static DbProviderFactory GetFactory(string providerName)
        {
            //a simple static manual override.
            if (_manualProviderFactory != null) return _manualProviderFactory;
            return DbProviderFactories.GetFactory(providerName);
        }


        /// <summary>
        /// Adds an existing factory. Call this before creating the DatabaseReader or SchemaReader.  Use with care!
        /// </summary>
        /// <param name="factory">The factory.</param>
        /// <exception cref="System.ArgumentNullException">schemaReader</exception>
        public static void AddFactory(DbProviderFactory factory)
        {
            if (factory == null) throw new ArgumentNullException("factory");
            _manualProviderFactory = factory;
        }


        /// <summary>
        /// List of all the valid Providers. Use the ProviderInvariantName to fill ProviderName property
        /// </summary>
        /// <returns></returns>
        public static DataTable Providers()
        {
            return DbProviderFactories.GetFactoryClasses();
        }
    }
}
