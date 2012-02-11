using System.Globalization;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    class DataAnnotationWriter
    {
        private readonly bool _isNet4;

        public DataAnnotationWriter(bool isNet4)
        {
            _isNet4 = isNet4;
        }

        public void Write(ClassBuilder cb, DatabaseColumn column)
        {
            if (_isNet4) //Display is .Net 4 and Silverlight 3 only 
            {
                var name = column.NetName;
                //http://weblogs.asp.net/jgalloway/archive/2005/09/27/426087.aspx
                name = Regex.Replace(name, "([A-Z]+|[0-9]+)", " $1", RegexOptions.Compiled).Trim();
                if (name != column.NetName)
                {
                    cb.AppendLine("[Display(Name=\"" + name + "\")]");
                }
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
                cb.AppendLine("[Required]");

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
                    cb.AppendLine(string.Format(CultureInfo.InvariantCulture, "[StringLength({0})]", column.Length));
            }
            else if (dt.IsInt)
            {
                var max = column.Precision.GetValueOrDefault() - column.Scale.GetValueOrDefault();
                if (max > 0 && max < 10)
                {
                    //int.MaxValue is 2,147,483,647 (precision 10), no need to range
                    cb.AppendLine(string.Format(CultureInfo.InvariantCulture, "[Range(0, {0})]", new string('9', max)));
                }
            }
            else if (dt.GetNetType() == typeof(decimal))
            {
                //[Range(typeof(decimal),"0", "999")]
                var max = column.Precision.GetValueOrDefault() - column.Scale.GetValueOrDefault();
                if (max > 0 && max < 28)
                {
                    cb.AppendLine(string.Format(CultureInfo.InvariantCulture, "[Range(typeof(decimal), \"0\", \"{0}\")]", new string('9', max)));
                }
            }

        }
    }
}
