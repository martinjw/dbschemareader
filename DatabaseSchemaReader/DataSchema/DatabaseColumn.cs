using System;
using System.Diagnostics;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A column in the database
    /// </summary>
    [Serializable]
    public class DatabaseColumn
    {
        #region Fields
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _netName;

        #endregion

        #region Basic Properties

        /// <summary>
        /// Gets or sets the column name (original database format)
        /// </summary>
        /// <value>
        /// The column name.
        /// </value>
        public string Name { get; set; }

        public string DbDataType { get; set; }

        public string ForeignKeyTableName { get; set; }

        public int? Length { get; set; }

        public bool Nullable { get; set; }

        public int Ordinal { get; set; }

        public string DefaultValue { get; set; }

        /// <summary>
        /// Precision is the number of digits in a number. For example, the number 123.45 has a precision of 5 and a scale of 2.
        /// </summary>
        /// <value>
        /// The precision.
        /// </value>
        public int? Precision { get; set; }

        /// <summary>
        /// Scale is the number of digits to the right of the decimal point in a number. For example, the number 123.45 has a precision of 5 and a scale of 2.
        /// </summary>
        /// <value>
        /// The scale.
        /// </value>
        public int? Scale { get; set; }

        public int? DateTimePrecision { get; set; }

        public string TableName { get; set; }

        #endregion

        #region Derived properties

        public bool IsForeignKey { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsIndexed { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsUniqueKey { get; set; }

        /// <summary>
        /// Gets or sets the column name in .Net (C#) compatible format.
        /// </summary>
        /// <value>
        /// The .net name
        /// </value>
        public string NetName
        {
            get { return _netName ?? Name; }
            set { _netName = value; }
        }

        /// <summary>
        /// Gets or sets the table reference. When converting from a database, only the string <see cref="TableName"/> is set; this object reference must be added (for example using <see cref="DatabaseSchemaFixer"/>)
        /// </summary>
        /// <value>
        /// The table.
        /// </value>
        public DatabaseTable Table { get; set; }

        public DatabaseSchema DatabaseSchema { get; set; }

        public DataType DataType { get; set; }

        public DatabaseTable ForeignKeyTable { get; set; }

        #endregion

        #region Utility methods


        public override string ToString()
        {
            return Name + " (" + DbDataType + ")"
                + (IsPrimaryKey ? " PK" : "")
                + (IsIdentity ? " Identity" : "");
        }

        #endregion
    }
}
