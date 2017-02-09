using DatabaseSchemaReader.DataSchema;
using System.Collections.Generic;

namespace SqlServerSchemaReader.Schema
{
    /// <summary>
    /// Alias data type, aka "User Defined Data Types".
    /// </summary>
    public class AliasType : NamedSchemaObject<AliasType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AliasType"/> class.
        /// </summary>
        public AliasType()
        {
            DependentColumns = new List<DatabaseColumn>();
            DependentArguments = new List<DatabaseArgument>();
        }

        /// <summary>
        /// Name of the underlying system type.
        /// </summary>
        public string SystemType { get; set; }

        /// <summary>
        /// The maximum length in bytes. -1 = varchar(max) or xml.
        /// 16 = text columns.
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// Nullable.
        /// </summary>
        public bool Nullable { get; set; }

        /// <summary>
        /// Precision is the number of digits in a number. For example, the number 123.45 has a precision of 5 and a scale of 2.
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// Scale is the number of digits to the right of the decimal point in a number. For example, the number 123.45 has a precision of 5 and a scale of 2.
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// Gets the dependent columns.
        /// </summary>
        public List<DatabaseColumn> DependentColumns { get; }

        /// <summary>
        /// Gets the dependent stored procedure arguments.
        /// </summary>
        public List<DatabaseArgument> DependentArguments { get; }
    }
}