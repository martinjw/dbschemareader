using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    class DataAnnotationWriter
    {
        private readonly bool _isNet4;
        private readonly CodeWriterSettings _codeWriterSettings;
        private string _friendlyName;

        public DataAnnotationWriter(bool isNet4, CodeWriterSettings codeWriterSettings)
        {
            _codeWriterSettings = codeWriterSettings;
            _isNet4 = isNet4;
        }

        public void Write(ClassBuilder cb, DatabaseColumn column, string propertyName)
        {
            var netName = column.NetName ?? column.Name;
            //http://weblogs.asp.net/jgalloway/archive/2005/09/27/426087.aspx
            _friendlyName = Regex.Replace(netName, "([A-Z]+|[0-9]+)", " $1", RegexOptions.Compiled).Trim();



            if (column.IsPrimaryKey)
            {
                cb.AppendLine("[Key]");
            }
            else if (!column.Nullable)
            {
                cb.AppendLine("[Required]");
            }

            WriteColumnAttribute(cb, column.Name);

            if (column.IsAutoNumber)
            {
                cb.AppendLine($"[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            }
            else if (!string.IsNullOrEmpty(column.DefaultValue))
            {
                cb.AppendLine($"[DatabaseGenerated(DatabaseGeneratedOption.Computed)]");
            }
        }

        private void WriteColumnAttribute(ClassBuilder cb, string name)
        {
            cb.AppendLine($"[Column(\"\\\"{name}\\\"\")]");
        }

        private static void WriteIndex(ClassBuilder cb, DatabaseColumn column)
        {
            //EF 6.1 [Index]
            var table = column.Table;
            //find all the indexes that contain this column
            var indexes = table.Indexes.FindAll(x => x.Columns.Contains(column));
            var pk = table.PrimaryKey;
            foreach (var index in indexes)
            {
                if (pk != null && pk.Columns.SequenceEqual(index.Columns.Select(c => c.Name)))
                {
                    //this is the primary key index
                    continue;
                }
                var sb = new StringBuilder();
                sb.Append("[Index(\"" + index.Name + "\"");
                var multiColumn = index.Columns.Count > 1;
                if (multiColumn)
                {
                    var position = index.Columns.FindIndex(x => Equals(x, column)) + 1;
                    sb.Append(", " + position);
                }

                if (index.IsUnique)
                {
                    //[Index("IdAndRating", 2, IsUnique = true)]
                    sb.Append(", IsUnique = true");
                }
                sb.Append(")]");
                cb.AppendLine(sb.ToString());
            }
        }

        private void WriteDecimalRange(ClassBuilder cb, int max)
        {
            var maximum = new string('9', max);
            var range = string.Format(CultureInfo.InvariantCulture, "[Range(typeof(decimal), \"0\", \"{0}\")]", maximum);
            var rangeErrorMessage = _codeWriterSettings.RangeErrorMessage;
            if (!string.IsNullOrEmpty(rangeErrorMessage))
            {
                range = range.Replace(")]",
                    ", ErrorMessage=\"" +
                    string.Format(CultureInfo.InvariantCulture, rangeErrorMessage, maximum, _friendlyName) +
                    "\")]");
            }
            cb.AppendLine(range);
        }

        private void WriteIntegerRange(ClassBuilder cb, int max)
        {
            var maximum = new string('9', max);
            var range = string.Format(CultureInfo.InvariantCulture, "[Range(0, {0})]", maximum);
            var rangeErrorMessage = _codeWriterSettings.RangeErrorMessage;
            if (!string.IsNullOrEmpty(rangeErrorMessage))
            {
                range = range.Replace(")]",
                    ", ErrorMessage=\"" +
                    string.Format(CultureInfo.InvariantCulture, rangeErrorMessage, maximum, _friendlyName) +
                    "\")]");
            }
            cb.AppendLine(range);
        }

        private void WriteStringLengthAttribute(ClassBuilder cb, int? length)
        {
            var stringLength = string.Format(CultureInfo.InvariantCulture, "[StringLength({0})]", length);
            var lengthErrorMessage = _codeWriterSettings.StringLengthErrorMessage;
            if (!string.IsNullOrEmpty(lengthErrorMessage))
            {
                stringLength = stringLength.Replace(")]",
                    ", ErrorMessage=\"" +
                    string.Format(CultureInfo.InvariantCulture, lengthErrorMessage, length, _friendlyName) +
                    "\")]");
            }
            cb.AppendLine(stringLength);
        }

        private void WriteRequiredAttribute(ClassBuilder cb)
        {
            var required = "[System.ComponentModel.DataAnnotations.Required]";
            var requiredErrorMessage = _codeWriterSettings.RequiredErrorMessage;
            if (!string.IsNullOrEmpty(requiredErrorMessage))
            {
                required = "[System.ComponentModel.DataAnnotations.Required(ErrorMessage=\"" +
                    string.Format(CultureInfo.InvariantCulture, requiredErrorMessage, _friendlyName) +
                    "\")]";
            }
            cb.AppendLine(required);
        }

        private void WriteDisplayAttribute(ClassBuilder cb, string name)
        {
            if (_friendlyName != name)
            {
                cb.AppendLine("[Display(Name=\"" + _friendlyName + "\")]");
            }
        }
    }
}
