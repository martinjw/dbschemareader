using System;
using System.Xml.Serialization;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// An argument (in or out parameter) to a stored procedure or function.
    /// </summary>
    [Serializable]
    public partial class DatabaseArgument : NamedObject<DatabaseArgument>
    {
        private DatabaseArgumentMode _argumentMode;

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
        /// Gets or sets the dataType (string format). See <see cref="DataType"/> or <see cref="UserDefinedTable"/>
        /// </summary>
        /// <value>The dataType.</value>
        public string DatabaseDataType { get; set; }

        /// <summary>
        /// If this is a table value parameter, the actual type
        /// </summary>
        public UserDefinedTable UserDefinedTable { get; set; }

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

        /// <summary>
        /// Gets or sets the mode (input/output/both or table)
        /// </summary>
        public DatabaseArgumentMode ArgumentMode
        {
            get => _argumentMode;
            set
            {
                _argumentMode = value;
                switch (value)
                {
                    case DatabaseArgumentMode.In:
                        In = true;
                        Out = false;
                        break;
                    case DatabaseArgumentMode.Out:
                        In = false;
                        Out = true;
                        break;
                    case DatabaseArgumentMode.InOut:
                        In = true;
                        Out = true;
                        break;
                    case DatabaseArgumentMode.Table:
                        In = false;
                        Out = true;
                        break;
                }
            }
        }

        #region Derived properties

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

        /// <summary>
        /// For PostgreSQL, the object id of the parent procedure or function
        /// </summary>
        public uint? Oid { get; set; }

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

    /// <summary>
    /// The argument mode- normally In, Out or InOut. Table is used by PostgreSQL and SAP.
    /// </summary>
    public enum DatabaseArgumentMode
    {
        /// <summary>
        /// Input parameter
        /// </summary>
        In,
        /// <summary>
        /// Output parameter (NB in SqlServer, output parameters are implicitly InOut)
        /// </summary>
        Out,
        /// <summary>
        /// Input and output parameter
        /// </summary>
        InOut,
        /// <summary>
        /// Table parameter (PostgreSQL, SAP)
        /// </summary>
        Table
    }
}
