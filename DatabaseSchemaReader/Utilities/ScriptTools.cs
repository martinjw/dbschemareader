using System;
using System.Linq;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    /// Help with scripts. Likely to be changed to do more simple scripting
    /// </summary>
    public static class ScriptTools
    {
        /// <summary>
        /// Splits the SQL server script using the GO lines.
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns></returns>
        /// <remarks>
        /// You can't execute a script with "GO" lines against SQLServer in ADO.
        /// This splits the sections into executable parts.
        /// </remarks>
        public static string[] SplitScript(string script)
        {
            var separator = new[] { Environment.NewLine + "GO" + Environment.NewLine };
            return (script + Environment.NewLine)
                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                //remove line feeds
                .Where(s => s.Trim().Length > 0)
                .ToArray();
        }

        /// <summary>
        /// Splits the script using the ; (required for Oracle)
        /// </summary>
        /// <param name="script">The script.</param>
        /// <returns></returns>
        public static string[] SplitBySemicolon(string script)
        {
            if (String.IsNullOrEmpty(script)) return new string[] { };

            var separator = new[] { ";" };
            return script.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                //remove line feeds
                .Select(s=> s.Trim())
                .Where(s => s.Length > 0)
                .ToArray();
        }
    }
}
