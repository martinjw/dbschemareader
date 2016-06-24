using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A column in the database
    /// </summary>
    [Serializable]
    public partial class DatabaseColumn : NamedSchemaObject<DatabaseColumn>
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DatabaseColumnIdentity _identityDefinition;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseColumn"/> class.
        /// </summary>
        public DatabaseColumn()
        {
            ForeignKeyTableNames = new List<string>();
        }

        #region Basic Properties

        /// <summary>
        /// Gets or sets the database data type as a string (as defined by the database platform)
        /// </summary>
        /// <value>
        /// The type of the db data.
        /// </value>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        public string DbDataType { get; set; }

        /// <summary>
        /// If <see cref="IsForeignKey"/> is true, gets or sets the name of the foreign key table. <see cref="ForeignKeyTable"/> may contain the actual table. If you have composite keys, consider <see cref="ForeignKeyTableNames"/> instead.
        /// </summary>
        /// <value>
        /// The name of the foreign key table.
        /// </value>
        public string ForeignKeyTableName
        {
            get { return ForeignKeyTableNames.FirstOrDefault(); }
            set { ForeignKeyTableNames.Add(value); }
        }


        /// <summary>
        /// If <see cref="IsForeignKey"/> is true, gets or sets the foreign key table names. If you have no composite keys, <see cref="ForeignKeyTableName" /> is sufficient.
        /// </summary>
        /// <value>
        /// The names of the foreign key table.
        /// </value>
        public List<string> ForeignKeyTableNames { get; private set; }

        /// <summary>
        /// Gets or sets the length if this is string (VARCHAR) or character (CHAR) type data. In SQLServer, a length of -1 indicates VARCHAR(MAX).
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is nullable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if nullable; otherwise, <c>false</c>.
        /// </value>
        public bool Nullable { get; set; }

        /// <summary>
        /// Gets or sets the ordinal (the order that the columns were defined in the database).
        /// </summary>
        /// <value>
        /// The ordinal.
        /// </value>
        public int Ordinal { get; set; }

        /// <summary>
        /// Gets or sets a default value for the column. May be null.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
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

        /// <summary>
        /// Gets or sets the date time precision (only Oracle).
        /// </summary>
        /// <value>
        /// The date time precision.
        /// </value>
        public int? DateTimePrecision { get; set; }

        /// <summary>
        /// Gets or sets the name of the parent table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the identity definition (if this is an Identity column). 
        /// Null if this is not an identity column (<see cref="IsAutoNumber"/> is false), 
        /// or the database uses another method of autonumbering (<see cref="DefaultValue"/> or sequences).
        /// </summary>
        public DatabaseColumnIdentity IdentityDefinition
        {
            get { return _identityDefinition; }
            set
            {
                _identityDefinition = value;
                if (_identityDefinition != null) IsAutoNumber = true;
            }
        }

        /// <summary>
        /// Gets or sets the "computed" (or "virtual") definition.
        /// </summary>
        /// <value>
        /// The computed definition.
        /// </value>
        public string ComputedDefinition { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        #endregion

        #region Derived properties

        /// <summary>
        /// Gets or sets a value indicating whether this column is a foreign key.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is foreign key; otherwise, <c>false</c>.
        /// </value>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this column is an autonumber column (identity or equivalent)
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this column is autonumber; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// If the database supports true Identity autonumbering, there should be more details in <see cref="IdentityDefinition"/>. If this is an equivalent (e.g. using sequences), then <see cref="IdentityDefinition"/> will be null.
        /// </remarks>
        public bool IsAutoNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is indexed.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is indexed; otherwise, <c>false</c>.
        /// </value>
        public bool IsIndexed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is part of the primary key.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is primary key; otherwise, <c>false</c>.
        /// </value>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is part of a unique key.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is unique key; otherwise, <c>false</c>.
        /// </value>
        public bool IsUniqueKey { get; set; }

        /// <summary>
        /// Gets or sets the column name in .Net (C#) compatible format.
        /// </summary>
        /// <value>
        /// The .net name
        /// </value>
        public string NetName { get; set; }

        /// <summary>
        /// Gets or sets the table reference. When converting from a database, only the string <see cref="TableName"/> is set; this object reference must be added (for example using <see cref="DatabaseSchemaFixer"/>)
        /// </summary>
        /// <value>
        /// The table.
        /// </value>
        [XmlIgnore]
        public DatabaseTable Table { get; set; }

        /// <summary>
        /// Gets or sets the database schema. May be null for some platforms (SQLite, SqlServer CE)
        /// </summary>
        /// <value>
        /// The database schema.
        /// </value>
        [XmlIgnore]
        public DatabaseSchema DatabaseSchema { get; set; }

        /// <summary>
        /// Gets or sets the type of the data.
        /// </summary>
        /// <value>
        /// The type of the data.
        /// </value>
        public DataType DataType { get; set; }

        /// <summary>
        /// If <see cref="IsForeignKey"/> is true, gets or sets the foreign key table. <see cref="ForeignKeyTableName"/> contains the name of the table.
        /// </summary>
        /// <value>
        /// The foreign key table.
        /// </value>
        [XmlIgnore]
        public DatabaseTable ForeignKeyTable { get; set; }

        #endregion

        #region Utility methods

        /// <summary>
        /// Gets a value indicating whether this column is "computed" or "virtual".
        /// </summary>
        /// <value>
        /// <c>true</c> if this column is computed; otherwise, <c>false</c>.
        /// </value>
        public bool IsComputed { get { return !string.IsNullOrEmpty(ComputedDefinition); } }



        /// <summary>
        /// Shallow clones this instance.
        /// </summary>
        /// <returns></returns>
        public DatabaseColumn Clone()
        {
            return (DatabaseColumn)MemberwiseClone();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name + " (" + DbDataType + ")"
                + (IsPrimaryKey ? " PK" : "")
                + (IsAutoNumber ? " AutoNumber" : "");
        }

        #endregion
    }
}
