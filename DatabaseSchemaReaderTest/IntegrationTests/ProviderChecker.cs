using System;
using System.Data.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// Ensures provider exists on test machine.
    /// </summary>
    static class ProviderChecker
    {
        /// <summary>
        /// Checks the specified provider name. If invalid, test is aborted with Inconclusive result.
        /// </summary>
        /// <param name="providerName">Name of the provider.</param>
        /// <param name="connectionString">The connection string.</param>
        public static void Check(string providerName, string connectionString)
        {
            DbProviderFactory factory = null;
            try
            {
                factory = DbProviderFactories.GetFactory(providerName);
            }
            catch (ArgumentException)
            {
                Assert.Inconclusive("Cannot test for provider " + providerName);
            }
            catch (System.Configuration.ConfigurationException)
            {
                Assert.Inconclusive("Cannot test for provider " + providerName);
            }
            catch (System.Reflection.TargetInvocationException exception)
            {
                //The provider is not compatible with the version of Oracle client (32bit/64bit)
                Assert.Inconclusive("Cannot access database for provider " + providerName +
                    " message= " + exception.Message);
            }

            try
            {
                using (var connection = factory.CreateConnection())
                {
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    connection.Close();
                }
            }
            catch (DbException exception)
            {
                Assert.Inconclusive("Cannot access database for provider " + providerName + " message= " +
                                    exception.Message);
            }
            catch (Exception exception)
            {
                //no oracle installed = System.Exception: System.Data.OracleClient requires Oracle client software version 8.1.7 or greater.
                Assert.Inconclusive("Cannot access database for provider " + providerName + " message= " + exception.Message);
            }
        }
    }
}
