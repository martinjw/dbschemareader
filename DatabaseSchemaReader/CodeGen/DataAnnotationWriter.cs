using System.Globalization;
using System.Text.RegularExpressions;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    static class DataAnnotationWriter
    {
        public static void Write(ClassBuilder cb, DatabaseColumn column)
        {
            var name = column.NetName;
            //http://weblogs.asp.net/jgalloway/archive/2005/09/27/426087.aspx
            name = Regex.Replace(name, "([A-Z]+|[0-9]+)", " $1", RegexOptions.Compiled).Trim();
            if (name != column.NetName)
            {
                //.Net 4 and Silverlight 3 only 
                cb.AppendLine("// [Display(Name=\"" + name + "\")]");
            }

            //we won't mark primary keys as required, because they may be assigned by a ORM primary key strategy or database identity/sequence
            if (column.IsPrimaryKey)
            {
                //.Net 4 and Silverlight 3 only 
                cb.AppendLine("// [Key]");
            }
            else if (!column.Nullable)
                cb.AppendLine("[Required]");

            //foreign keys will not expose the underlying type
            if (column.IsForeignKey)
                return;

            var dt = column.DataType;
            if (dt.IsString)
            {
                //if it's over a million characters, no point validating
                if (column.Length < 1073741823)
                    cb.AppendLine(string.Format(CultureInfo.InvariantCulture, "[StringLength({0})]", column.Length));
            }
            else if (dt.IsInt)
            {
                var max = column.Precision.GetValueOrDefault() - column.Scale.GetValueOrDefault();
                if (max > 0)
                {
                    cb.AppendLine(string.Format(CultureInfo.InvariantCulture, "[Range(0, {0})]", new string('9', max)));
                }
            }
            else if (dt.GetNetType() == typeof(decimal))
            {
                //[Range(typeof(decimal),"0", "999")]
                var max = column.Precision.GetValueOrDefault() - column.Scale.GetValueOrDefault();
                if (max > 0)
                {
                    cb.AppendLine(string.Format(CultureInfo.InvariantCulture, "[Range(typeof(decimal), \"0\", \"{0}\")]", new string('9', max)));
                }
            }

        }
    }
}
