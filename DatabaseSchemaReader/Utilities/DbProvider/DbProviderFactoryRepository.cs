using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DatabaseSchemaReader.Utilities.DbProvider
{
    /// <summary>
    ///Extension of DbProviderFactories for allowing programmatically adding external dll dataprovider which are not
    ///declared at app.config or machine.config. Basically extracted from
    ///http://sandrinodimattia.net/dbproviderfactoryrepository-managing-dbproviderfactories-in-code/
    /// </summary>
    public class DbProviderFactoryRepository
    {
        /// <summary>
        ///The table containing all the data.
        /// </summary>
        private DataTable _dbProviderFactoryTable;

        /// <summary>
        ///Initialize the repository.
        /// </summary>
        public DbProviderFactoryRepository()
        {
            LoadDbProviderFactories();
        }

        /// <summary>
        ///Gets all providers.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DbProviderFactoryDescription> GetAllDescriptions()
        {
            return _dbProviderFactoryTable.Rows.Cast<DataRow>().Select(o => new DbProviderFactoryDescription(o));
        }

        /// <summary>
        ///Get provider by invariant.
        /// </summary>
        /// <param name="invariant"></param>
        /// <returns></returns>
        public DbProviderFactoryDescription GetDescriptionByInvariant(string invariant)
        {
            var row =
                _dbProviderFactoryTable.Rows.Cast<DataRow>()
                    .FirstOrDefault(o => o["InvariantName"] != null && o["InvariantName"].ToString() == invariant);
            return row != null ? new DbProviderFactoryDescription(row) : null;
        }

        /// <summary>
        ///Gets the factory.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns></returns>
        public DbProviderFactory GetFactory(DbProviderFactoryDescription description)
        {
            var providerType = AssemblyHelper.LoadTypeFrom(description.AssemblyQualifiedName);
            if (providerType == null) return null;

            var providerInstance = providerType.GetField("Instance",
                BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
            if (providerInstance == null) return null;
            if (!providerInstance.FieldType.IsSubclassOf(typeof(DbProviderFactory))) return null;
            try
            {
                var factory = providerInstance.GetValue(null);
                return factory != null ? (DbProviderFactory)factory : null;
            }
            catch (TargetInvocationException)
            {
                return null;
            }
        }

        /// <summary>
        ///Gets the factory.
        /// </summary>
        /// <param name="invariant">The invariant.</param>
        /// <returns></returns>
        public DbProviderFactory GetFactory(string invariant)
        {
            if (string.IsNullOrEmpty(invariant))
            {
                throw new ArgumentNullException("invariant");
            }

            var desc = GetDescriptionByInvariant(invariant);
            return desc != null ? GetFactory(desc) : null;
        }

        /// <summary>
        ///Loads the external database provider assemblies.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">$Path does not {path} exist.</exception>
        public void LoadExternalDbProviderAssemblies(string path)
        {
            LoadExternalDbProviderAssemblies(path, true);
        }

        /// <summary>
        ///Loads the external database provider assemblies.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="includeSubfolders">if set to <c>true</c> [include subfolders].</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">$Path does not {path} exist.</exception>
        public void LoadExternalDbProviderAssemblies(string path, bool includeSubfolders)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }

            if (!Directory.Exists(path))
            {
                throw new ArgumentException(String.Format("Path {0} does not exist.", path), "path");
            }

            // main directory
            var mainDirectory = new DirectoryInfo(path);
            var directories = new List<DirectoryInfo> { mainDirectory };

            // also search in direct subfolders
            if (includeSubfolders)
            {
                directories.AddRange(mainDirectory.GetDirectories());
            }

            // iterate over all directories and search for dll libraries
            foreach (var directory in directories)
            {
                foreach (var file in directory.GetFiles().Where(file =>
                    String.Equals(file.Extension, ".dll", StringComparison.OrdinalIgnoreCase)))
                {
                    // This will work to load only the file from other directory without dependencies! But at access time the dependecies are necessary!
                    //var assembly = Assembly.LoadFile(file.FullName);

                    // Load all assemblies from directory in current AppDomain. This is necessary for accessing all types. Other
                    // opertunities like Assembly.LoadFile will only load one file temporary (later access will not have dependecy finding)
                    // and Assembly.ReflectionOnlyLoad will load all dependencies at beginning what will not work in other directories as bin.
                    AssemblyName assemblyName;
                    try
                    {
                        assemblyName = AssemblyName.GetAssemblyName(file.FullName);
                    }
                    catch (BadImageFormatException)
                    {
                        //dll isn't .net (eg SQLite.Interop.dll)
                        continue;
                    }

                    Assembly assembly;
                    try
                    {
                        assembly = AppDomain.CurrentDomain.Load(assemblyName);
                    }
                    catch (Exception exception)
                    {
                        Trace.TraceError($"Could not load {assemblyName} - {exception.Message}");
                        continue;
                    }

                    foreach (var type in assembly.GetLoadableTypes())
                    {
                        if (type.IsClass)
                        {
                            if (typeof(DbProviderFactory).IsAssignableFrom(type))
                            {
                                // Ignore already existing provider
                                if (GetDescriptionByInvariant(type.Namespace) == null)
                                {
                                    var newDescription = new DbProviderFactoryDescription
                                    {
                                        Description = ".Net Framework Data Provider for " + type.Name,
                                        InvariantName = type.Namespace,
                                        Name = type.Name + " Data Provider",
                                        AssemblyQualifiedName = type.AssemblyQualifiedName
                                    };
                                    Add(newDescription);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///Adds the specified provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public void Add(DbProviderFactoryDescription provider)
        {
            Delete(provider);
            _dbProviderFactoryTable.Rows.Add(provider.Name, provider.Description, provider.InvariantName, provider.AssemblyQualifiedName);
        }

        /// <summary>
        ///Deletes the specified provider if present.
        /// </summary>
        /// <param name="provider">The provider.</param>
        private void Delete(DbProviderFactoryDescription provider)
        {
            var row =
                _dbProviderFactoryTable.Rows.Cast<DataRow>()
                    .FirstOrDefault(o => o["InvariantName"] != null && o["InvariantName"].ToString() == provider.InvariantName);
            if (row != null)
            {
                _dbProviderFactoryTable.Rows.Remove(row);
            }
        }

        /// <summary>
        ///Opens the table.
        /// </summary>
        private void LoadDbProviderFactories()
        {
            _dbProviderFactoryTable = DbProviderFactories.GetFactoryClasses();
        }
    }

    internal static class AssemblyExtensions
    {
        /// <summary>
        /// Gets the loadable types.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">assembly</exception>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}