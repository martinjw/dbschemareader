using System;

namespace DatabaseSchemaReader.Utilities
{
    /// <summary>
    /// Help with SqlServer scripts. Likely to be changed to do more simple scripting
    /// </summary>
    public static class SqlServerScriptTools
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
            return script.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
