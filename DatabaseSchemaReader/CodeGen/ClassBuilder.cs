using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Writes a class while retaining indenting
    /// </summary>
    public class ClassBuilder
    {
        readonly StringBuilder _sb = new StringBuilder();
        private string _indent = string.Empty;
        private int _indentLevel;

        /// <summary>
        /// Appends the line.
        /// </summary>
        /// <param name="s">The string.</param>
        public void AppendLine(string s)
        {
            _sb.AppendLine(_indent + s);
        }

        /// <summary>
        /// Appends the format.
        /// </summary>
        /// <param name="s">The string.</param>
        /// <param name="args">The arguments.</param>
        public void AppendFormat(string s, params object[] args)
        {
            _sb.AppendLine(_indent + string.Format(CultureInfo.InvariantCulture, s, args));
        }

        internal void AppendXmlSummary(string summary, string returns = "", string remarks = "", IEnumerable<Tuple<string, string>> exceptions = null, IEnumerable<Tuple<string, string>> parameters = null)
        {
            if (string.IsNullOrEmpty(summary)) return;
            _sb.AppendLine(_indent + "/// <summary>");
            _sb.AppendLine(_indent + "/// " + summary);
            _sb.AppendLine(_indent + "/// </summary>");
            if (parameters?.Count() > 0)
            {
                foreach (var p in parameters)
                {
                    _sb.AppendLine($"{_indent}/// <param name=\"{p.Item1}\">{p.Item2}</param>");
                }
            }

            if (!string.IsNullOrEmpty(returns))
            {
                _sb.AppendLine($"{_indent}/// <returns>{returns}</returns>");
            }

            if (exceptions?.Count() > 0)
            {
                foreach (var e in exceptions)
                {
                    _sb.AppendLine($"{_indent}/// <exception cref=\"{e.Item1}\">{e.Item2}</exception>");
                }
            }

            if (!string.IsNullOrEmpty(remarks))
            {
                _sb.AppendLine($"{_indent}/// <remarks>{remarks}</remarks>");
            }
        }

        internal Nester BeginNest(string s)
        {
            return BeginNest(s, null);
        }

        internal Nester BeginNest(string s, string summary)
        {
            //_sb.AppendLine();
            //AppendXmlSummary(summary);
            _sb.AppendLine(_indent + s);
            _sb.AppendLine(_indent + "{");
            PushIndent();
            return new Nester(this);
        }

        internal Nester BeginBrace(string s)
        {
            //simple bracing, no leading line
            _sb.AppendLine(_indent + s);
            _sb.AppendLine(_indent + "{");
            PushIndent();
            return new Nester(this);
        }

        internal void AppendAutomaticProperty(string dataType, string propertyName)
        {
            AppendAutomaticProperty(dataType, propertyName, true);
        }

        internal void AppendAutomaticProperty(string dataType, string propertyName, bool isVirtual)
        {
            var line = string.Format(
                CultureInfo.InvariantCulture,
                "{0}public {1}{2} {3} {{ get; set; }}",
                _indent,
                isVirtual ? "virtual " : string.Empty,
                dataType,
                propertyName);

            _sb.AppendLine(line);
            _sb.AppendLine(); //add an empty line
        }

        internal void AppendAutomaticCollectionProperty(string dataType, string propertyName)
        {
            AppendAutomaticCollectionProperty(dataType, propertyName, false);
        }
        internal void AppendAutomaticCollectionProperty(string dataType, string propertyName, bool protectedSetter)
        {
            var line = string.Format(
                CultureInfo.InvariantCulture,
                "{0}public virtual {1} {2} {{ get; {3} set; }}",
                _indent,
                dataType,
                propertyName,
                //Starting with NH 3.2, setters must be protected, not private
                protectedSetter ? "protected" : "private");

            _sb.AppendLine(line);
            //_sb.AppendLine(); //add an empty line
        }

        internal void EndNest()
        {
            PopIndent(); //pop before writing close brace
            _sb.AppendLine(_indent + "}");
        }

        private void PushIndent()
        {
            _indentLevel++;
            _indent = new string(' ', _indentLevel * 4);
        }
        private void PopIndent()
        {
            _indentLevel--;
            _indent = new string(' ', _indentLevel * 4);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents the source code.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents the source code.
        /// </returns>
        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
