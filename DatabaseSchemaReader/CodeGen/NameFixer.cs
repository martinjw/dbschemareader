using System;
using System.CodeDom.Compiler;
using System.Globalization;
using System.Text.RegularExpressions;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Fixes database names to be pascal case and singular.
    /// Consider replacing this with something a little more powerful- eg Castle Project inflector 
    /// https://github.com/castleproject/Castle.ActiveRecord/blob/master/src/Castle.ActiveRecord/Framework/Internal/Inflector.cs
    /// </summary>
    public static class NameFixer
    {
        private static readonly CodeDomProvider CSharpProvider = CodeDomProvider.CreateProvider("C#");

        /// <summary>
        /// Fixes the specified name to be pascal cased and (crudely) singular.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <remarks>
        /// See C# language specification http://msdn.microsoft.com/en-us/library/aa664670.aspx
        /// </remarks>
        public static string ToPascalCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return "A" + Guid.NewGuid().ToString("N");

            var endsWithId = Regex.IsMatch(name, "[a-z0-9 _]{1}(?<Id>ID)$");

            name = MakePascalCase(name);
            name = MakeSingular(name);

            if (endsWithId)
            {
                //ends with a capital "ID" in an otherwise non-capitalized word
                name = name.Substring(0, name.Length - 2) + "Id";
            }

            //remove all spaces
            name = Regex.Replace(name, @"[^\w]+", string.Empty);
            return name;
        }

        /// <summary>
        /// Fixes the specified name to be camel cased. No singularization.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static string ToCamelCase(string name)
        {
            if (string.IsNullOrEmpty(name)) return "a" + Guid.NewGuid().ToString("N");

            var endsWithId = Regex.IsMatch(name, "[a-z0-9 _]{1}(?<Id>ID)$");

            name = MakePascalCase(name); //reuse this

            if (endsWithId)
            {
                //ends with a capital "ID" in an otherwise non-capitalized word
                name = name.Substring(0, name.Length - 2) + "Id";
            }

            //remove all spaces
            name = Regex.Replace(name, @"[^\w]+", string.Empty);

            if (Char.IsUpper(name[0]))
            {
                name = char.ToLowerInvariant(name[0]) +
                    (name.Length > 1 ? name.Substring(1) : string.Empty);
            }

            //this could still be a c# keyword
            if (!CSharpProvider.IsValidIdentifier(name))
            {
                //in practice all keywords are lowercase. 
                name = "@" + name;
            }
            return name;
        }

        private static string MakePascalCase(string name)
        {
            //make underscores into spaces, plus other odd punctuation
            name = name.Replace('_', ' ').Replace('$', ' ').Replace('#', ' ');

            //if it's all uppercase
            if (Regex.IsMatch(name, @"^[A-Z0-9 ]+$"))
            {
                //lowercase it
                name = CultureInfo.InvariantCulture.TextInfo.ToLower(name);
            }

            //if it's mixed case with no spaces, it's already pascal case
            if (name.IndexOf(' ') == -1 && !Regex.IsMatch(name, @"^[a-z0-9]+$"))
            {
                return name;
            }

            //titlecase it (words that are uppered are preserved)
            name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name);

            return name;
        }

        /// <summary>
        /// Very simple singular inflections. "Works on my database" (TM)
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private static string MakeSingular(string name)
        {
            if (name.EndsWith("ss", StringComparison.OrdinalIgnoreCase))
            {
                //ok, don't do anything. "Address" + X"ness" are valid singular
            }
            else if (name.EndsWith("us", StringComparison.OrdinalIgnoreCase))
            {
                //ok, don't do anything. "Status" + "Virus" are valid singular
            }
            else if (name.EndsWith("ses", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 2); //"Buses". Fails "Analyses" and "Cheeses". 
            }
            else if (name.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 3) + "y"; //"Territories", "Categories"
            }
            else if (name.EndsWith("xes", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 3) + "x"; //"Boxes"
            }
            else if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                name = name.Substring(0, name.Length - 1);
            }
            else if (name.Equals("People", StringComparison.OrdinalIgnoreCase))
            {
                name = "Person"; //add other irregulars.
            }
            return name;
        }
    }
}
