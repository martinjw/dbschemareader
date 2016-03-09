using System;
using DatabaseSchemaReader.Utilities;
#if !NUNIT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#endif

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
            DbProviderFactoryRepository.Add(manualDescription);

            var descs = DbProviderFactoryRepository.GetAllDescriptions();
            foreach (var description in descs)
            {
                //get the description individually
                var desc = DbProviderFactoryRepository.GetDescriptionByInvariant(description.InvariantName);
                Assert.AreEqual(description.AssemblyQualifiedName, desc.AssemblyQualifiedName);

                //get a factory
                var factory = DbProviderFactoryRepository.GetFactory(desc);
                //may be null if not accessible
            }

            //look in the current directory
            DbProviderFactoryRepository.LoadExternalDbProviderAssemblies(Environment.CurrentDirectory);
        }
    }
}