using System;

namespace DatabaseSchemaReader.SqlGen
{
    /// <summary>
    /// Generate simple CRUD stored procedures
    /// </summary>
    public interface IProcedureGenerator
    {
        /// <summary>
        /// Optionally override how column parameters are formatted
        /// </summary>
        /// <value>The format parameter function.</value>
        Func<string, string> FormatParameter { get; set; }

        /// <summary>
        /// Gets or sets the manual prefix.
        /// </summary>
        /// <value>The manual prefix.</value>
        string ManualPrefix { get; set; }

        /// <summary>
        /// Gets or sets the suffix.
        /// </summary>
        /// <value>The suffix.</value>
        string Suffix { get; set; }

        /// <summary>
        /// Gets the prefix.
        /// </summary>
        /// <value>The prefix.</value>
        string Prefix { get; }

        /// <summary>
        /// Writes to folder.
        /// </summary>
        /// <param name="path">The path.</param>
        void WriteToFolder(string path);

        /// <summary>
        /// Writes to script.
        /// </summary>
        /// <param name="scriptPath">The script path.</param>
        void WriteToScript(string scriptPath);

        /// <summary>
        /// Gets or sets the name of the cursor parameter. In Oracle, defaults to Result.
        /// </summary>
        /// <value>The name of the cursor parameter.</value>
        string CursorParameterName { get; set; }
    }
}