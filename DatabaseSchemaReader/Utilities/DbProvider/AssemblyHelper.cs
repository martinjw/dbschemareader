using System;
using System.Linq;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    ///     Helper class for assembly access.
    /// </summary>
    public static class AssemblyHelper
    {
        /// <summary>
        ///     Loads the type from an assembly qualified name. This allows loading types also if there are loaded dynamically
        ///     before by Assembly.LoadFile method.
        /// </summary>
        /// <param name="assemblyQualifiedName">Name of the assembly qualified.</param>
        /// <returns></returns>
        public static Type LoadTypeFrom(string assemblyQualifiedName)
        {
            // This will return null
            // Just here to test that the simple GetType overload can't return the actual type
            var t0 = Type.GetType(assemblyQualifiedName);

			#if NET4
            // Throws exception is type was not found
            return Type.GetType(
                assemblyQualifiedName,
                name =>
                {
                    // Returns the assembly of the type by enumerating loaded assemblies
                    // in the app domain            
                    return AppDomain.CurrentDomain.GetAssemblies().Where(z => z.FullName == name.FullName)
                        .FirstOrDefault();
                },
                null,
                true);
            #else
            return t0;
            #endif
        }
    }
}