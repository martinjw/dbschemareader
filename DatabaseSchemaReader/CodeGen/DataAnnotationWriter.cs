using System.Globalization;
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

        public void Write(ClassBuilder cb, DatabaseColumn column)
        {
            var netName = column.NetName ?? column.Name;
            //http://weblogs.asp.net/jgalloway/archive/2005/09/27/426087.aspx
            _friendlyName = Regex.Replace(netName, "([A-Z]+|[0-9]+)", " $1", RegexOptions.Compiled).Trim();

            if (_isNet4) //Display is .Net 4 and Silverlight 3 only 
            {
                WriteDisplayAttribute(cb, netName);
            }

            //we won't mark primary keys as required, because they may be assigned by a ORM primary key strategy or database identity/sequence
            if (column.IsPrimaryKey)
            {
                //.Net 4 and Silverlight 3 only 
                //NOTE: for EF CodeFirst generation, we also mapped fluently.
                //Despite the duplication, it's useful to have the key as a marker in the model
                if (_isNet4) cb.AppendLine("[Key]");
            }
            else if (!column.Nullable)
                WriteRequiredAttribute(cb);

            //foreign keys will not expose the underlying type
            if (column.IsForeignKey)
                return;

            var dt = column.DataType;
            if (dt == null)
            {
                //it is a database specific type
            }
            else if (dt.IsString)
            {
                //if it's over a million characters, no point validating
                if (column.Length < 1073741823 && column.Length > 0)
                    WriteStringLengthAttribute(cb, column.Length);
            }
            else if (dt.IsInt)
            {
                var max = column.Precision.GetValueOrDefault() - column.Scale.GetValueOrDefault();
                if (max > 0 && max < 10)
                {
                    //int.MaxValue is 2,147,483,647 (precision 10), no need to range
                    WriteIntegerRange(cb, max);
                }
            }
            else if (dt.GetNetType() == typeof(decimal))
            {
                //[Range(typeof(decimal),"0", "999")]
                var max = column.Precision.GetValueOrDefault() - column.Scale.GetValueOrDefault();
                if (max > 0 && max < 28)
                {
                    WriteDecimalRange(cb, max);
                }
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
            var required = "[Required]";
            var requiredErrorMessage = _codeWriterSettings.RequiredErrorMessage;
            if (!string.IsNullOrEmpty(requiredErrorMessage))
            {
                required = "[Required(ErrorMessage=\"" +
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
