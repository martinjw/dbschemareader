using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DatabaseSchemaReader.Compare
{
    static class SanitizeSql
    {
        static readonly Regex FindComments = new Regex("/\\*(?:.|[\\n\\r])*?\\*/", RegexOptions.Compiled);

        public static string StripComments(string sql)
        {
            //http://ostermiller.org/findcomment.html remove multiline comments
            sql = FindComments.Replace(sql, string.Empty);

            // remove empty lines and -- single line comments
            // ignore multi-line comments /* */
            var sb = new StringBuilder();
            using (var sr = new StringReader(sql))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.TrimStart().StartsWith("--", StringComparison.OrdinalIgnoreCase))
                    {
                        line = string.Empty;
                    }
                    if (!string.IsNullOrEmpty(line)) sb.AppendLine(line);
                }
            }
            return sb.ToString().Trim();
        }

    }
}
