using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;

namespace DatabaseSchemaReader.Utilities.DbProvider
{
    /// <summary>
    /// Tools to help with DbProviderFactory
    /// </summary>
    public static class FactoryTools
    {
        /// <summary>
        /// Finds the factory. You can override with <see cref="P:SingleProviderFactory"/> (simple) or <see cref="P:ProviderRespository"/>
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <returns></returns>
        public static DbProviderFactory GetFactory(string providerName)
        {
            //a simple static manual override.
            if (SingleProviderFactory != null) return SingleProviderFactory;
            if (ProviderRepository != null) return ProviderRepository.GetFactory(providerName);
            try
            {
                return DbProviderFactories.GetFactory(providerName);
            }
            catch (ArgumentException e)
            {
                Trace.TraceError($"Could not find installed provider {providerName}");
                return null;
            }
        }


        /// <summary>
        /// Adds an existing factory. Call this before creating the DatabaseReader or SchemaReader.  Use with care!
        /// </summary>
        public static DbProviderFactory SingleProviderFactory { get; set; }

        /// <summary>
        /// Gets or sets a provider repository.
        /// </summary>
        /// <value>
        /// The provider repository.
        /// </value>
        public static DbProviderFactoryRepository ProviderRepository { get; set; }


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
