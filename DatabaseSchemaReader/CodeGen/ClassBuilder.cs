using System.Globalization;
using System.Text;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Writes a class while retaining indenting
    /// </summary>
    class ClassBuilder
    {
        readonly StringBuilder _sb = new StringBuilder();
        private string _indent = string.Empty;
        private int _indentLevel;

        public void AppendLine(string s)
        {
            _sb.AppendLine(_indent + s);
        }

        public void AppendFormat(string s, params object[] args)
        {
            _sb.AppendLine(_indent + string.Format(CultureInfo.InvariantCulture, s, args));
        }

        public void AppendXmlSummary(string summary)
        {
            if (string.IsNullOrEmpty(summary)) return;
            _sb.AppendLine(_indent + "/// <summary>");
            _sb.AppendLine(_indent + "/// " + summary);
            _sb.AppendLine(_indent + "/// </summary>");
        }

        public Nester BeginNest(string s)
        {
            return BeginNest(s, null);
        }

        public Nester BeginNest(string s, string summary)
        {
            _sb.AppendLine();
            AppendXmlSummary(summary);
            _sb.AppendLine(_indent + s);
            _sb.AppendLine(_indent + "{");
            PushIndent();
            return new Nester(this);
        }

        public Nester BeginBrace(string s)
        {
            //simple bracing, no leading line
            _sb.AppendLine(_indent + s);
            _sb.AppendLine(_indent + "{");
            PushIndent();
            return new Nester(this);
        }

        public void AppendAutomaticProperty(string dataType, string propertyName)
        {
            AppendAutomaticProperty(dataType, propertyName, true);
        }

        public void AppendAutomaticProperty(string dataType, string propertyName, bool isVirtual)
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

        public void AppendAutomaticCollectionProperty(string dataType, string propertyName)
        {
            AppendAutomaticCollectionProperty(dataType, propertyName, false);
        }
        public void AppendAutomaticCollectionProperty(string dataType, string propertyName, bool protectedSetter)
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
            _sb.AppendLine(); //add an empty line
        }

        public void EndNest()
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

        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}
