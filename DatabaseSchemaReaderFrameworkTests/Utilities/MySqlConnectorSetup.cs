using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.Utilities.DbProvider;
using System;
using System.Collections.Generic;
using System.Configuration.Provider;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseSchemaReaderFrameworkTests.Utilities
{
    internal class MySqlConnectorSetup
    {
        private const string ProviderName = "MySql.Data.MySqlClient";
        private readonly string _connectionString = ConnectionStrings.MySql;
        private DatabaseTable _categoriesTable;
        private DbProviderFactory _factory;

        public DbProviderFactory EnsureProviderFactory()
        {
            _factory = MySqlConnector.MySqlConnectorFactory.Instance;
            //if (FactoryTools.GetFactory(ProviderName) == null)
            //{
                var manualDescription = new DbProviderFactoryDescription
                {
                    Description = ProviderName,
                    InvariantName = ProviderName,
                    Name = ProviderName,
                    AssemblyQualifiedName = _factory.GetType().AssemblyQualifiedName,
                };

                // Initialize the repository.
                if (FactoryTools.ProviderRepository == null)
                {
                    FactoryTools.ProviderRepository = new DbProviderFactoryRepository();
                }

                FactoryTools.ProviderRepository.Add(manualDescription);
            //}

            return _factory;
        }
    }
}
