using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// An argument (in or out parameter) to a stored procedure or function.
    /// </summary>
    [Serializable]
    public partial class DatabaseArgument : NamedObject
    {

        /// <summary>
        /// Gets or sets the schema owner.
        /// </summary>
        /// <value>
        /// The schema owner.
        /// </value>
        public string SchemaOwner { get; set; }

        /// <summary>
        /// Gets or sets the name of the procedure.
        /// </summary>
        /// <value>
        /// The name of the procedure.
        /// </value>
        public string ProcedureName { get; set; }
        /// <summary>
        /// Gets or sets the name of the package (only used by Oracle).
        /// </summary>
        /// <value>
        /// The name of the package.
        /// </value>
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the dataType (string format).
        /// </summary>
        /// <value>The dataType.</value>
        public string DatabaseDataType { get; set; }

        /// <summary>
        /// Gets or sets the ordinal position of the argument (1st, 2nd and so on).
        /// </summary>
        /// <value>
        /// The ordinal.
        /// </value>
        public decimal Ordinal { get; set; }
        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        /// <value>
        /// The precision.
        /// </value>
        public int? Precision { get; set; }
        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public int? Scale { get; set; }
        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int? Length { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this is an input (or input output) parameter
        /// </summary>
        /// <value>
        ///   <c>true</c> if in; otherwise, <c>false</c>.
        /// </value>
        public bool In { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this is an output (or input output) parameter
        /// </summary>
        /// <value>
        ///   <c>true</c> if out; otherwise, <c>false</c>.
        /// </value>
        public bool Out { get; set; }

        #region Derived properties

        /// <summary>
        /// Gets or sets the database schema.
        /// </summary>
        /// <value>
        /// The database schema.
        /// </value>
        public DatabaseSchema DatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the dataType. MAY BE NULL (eg Oracle REF CURSOR is not in datatypes list) - in which case refer to the string <see cref="DatabaseDataType"/>.
        /// </summary>
        /// <value>The dataType.</value>
        public DataType DataType { get; set; }

        /// <summary>
        /// Gets or sets the name of a .Net argument representing this specific argument.
        /// </summary>
        /// <value>
        /// The name of the net.
        /// </value>
        public string NetName { get; set; }

        #endregion

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

    }
}
