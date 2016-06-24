using System;
using System.Data.Common;
using System.Data.SqlClient;
using DatabaseSchemaReader;
using DatabaseSchemaReader.Utilities;
using DatabaseSchemaReader.Utilities.DbProvider;
using DatabaseSchemaReaderTest.IntegrationTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.Utilities
{
    [TestClass]
    public class DbProviderFactoryRepositoryTest
    {
        [TestMethod]
        public void TestRepository()
        {
            // Create a description manually and add it to the repository.
            var manualDescription = new DbProviderFactoryDescription
            {
                Description = ".NET Framework Data Provider for SuperDuperDatabase",
                InvariantName = "SuperDuperDatabase",
                Name = "SuperDuperDatabase Data Provider",
                AssemblyQualifiedName = "SuperDuperDatabase.SuperDuperProviderFactory, SuperDuperDatabase",
            };

            // Initialize the repository.
            var repo = new DbProviderFactoryRepository();
            repo.Add(manualDescription);

            var descs = repo.GetAllDescriptions();
            foreach (var description in descs)
            {
                //get the description individually
                var desc = repo.GetDescriptionByInvariant(description.InvariantName);
                Assert.AreEqual(description.AssemblyQualifiedName, desc.AssemblyQualifiedName);

                //get a factory
                var factory = repo.GetFactory(desc);
                //may be null if not accessible
            }

            //look in the current directory
            repo.LoadExternalDbProviderAssemblies(Environment.CurrentDirectory);
        }

        [TestMethod]
        public void FactoryToolsTest()
        {
            const string providername = "System.Data.SqlClient";

            //this is normally used
            var provider = FactoryTools.GetFactory(providername);
            Assert.AreEqual("System.Data.SqlClient.SqlClientFactory", provider.GetType().FullName, "No override, returns SqlClient");

            //override with a repository
            FactoryTools.ProviderRepository = new DbProviderFactoryRepository();
            var manualDescription = new DbProviderFactoryDescription
            {
                Description = ".NET Framework Data Provider for SuperDuperDatabase",
                InvariantName = "SuperDuperDatabase",
                Name = "SuperDuperDatabase Data Provider",
                AssemblyQualifiedName = typeof(SuperDuperProviderFactory).AssemblyQualifiedName,
            };
            FactoryTools.ProviderRepository.Add(manualDescription);

            provider = FactoryTools.GetFactory(providername);
            Assert.AreEqual("System.Data.SqlClient.SqlClientFactory", provider.GetType().FullName, "Overridden, but returns underlying SqlClient");
            provider = FactoryTools.GetFactory("SuperDuperDatabase");
            Assert.AreEqual(typeof(SuperDuperProviderFactory), provider.GetType(), "Overridden, returns manually added provider");

            //override with a single provider
            FactoryTools.SingleProviderFactory = SqlClientFactory.Instance;
            provider = FactoryTools.GetFactory("Xxxx");
            Assert.AreEqual("System.Data.SqlClient.SqlClientFactory", provider.GetType().FullName, "Overridden, always returns SqlClient");

            ProviderChecker.Check(providername, ConnectionStrings.Northwind);

            var dr = new DatabaseReader(ConnectionStrings.Northwind, "Xxxxx");
            var tables = dr.TableList();

            Assert.IsTrue(tables.Count > 0, "We called the reader with a bogus provider type, but we got the overridden type");
        }

        [TestCleanup]
        public void CleanUp()
        {
            //reset the overrides
            FactoryTools.ProviderRepository = null;
            FactoryTools.SingleProviderFactory = null;
        }

        public class SuperDuperProviderFactory : DbProviderFactory
        {
            public static SuperDuperProviderFactory Instance = new SuperDuperProviderFactory();
        }
    }
}