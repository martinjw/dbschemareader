using System;
using System.Linq;

namespace DatabaseSchemaReader.Utilities.DbProvider
{
    /// <summary>
    ///     Helper class for assembly access.
    /// </summary>
    internal static class AssemblyHelper
    {
        /// <summary>
        ///     Loads the type from an assembly qualified name. This allows loading types also if there are loaded dynamically
        ///     before by Assembly.LoadFile method.
        /// </summary>
        /// <param name="assemblyQualifiedName">Name of the assembly qualified.</param>
        /// <returns></returns>
        public static Type LoadTypeFrom(string assemblyQualifiedName)
        {
            // load type from assembly
            var type = Type.GetType(assemblyQualifiedName);

#if NET4
            if (type == null)
            {
                // load type from assembly with custom assembly resolver
                type = Type.GetType(
                    assemblyQualifiedName,
                    name =>
                    {
                        // Returns the assembly of the type by enumerating loaded assemblies
                        // in the app domain            
                        return AppDomain.CurrentDomain.GetAssemblies()
                            .FirstOrDefault(z => z.FullName == name.FullName);
                    },
                    null,
                    false);  
            }
#endif
            return type;
        }
    }
}