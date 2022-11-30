using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// A user defined data type (DOMAIN in postgreSql).
    /// </summary>
    [Serializable]
    public class UserDataType : NamedSchemaObject<UserDataType>
    {
        //inherit from DataType? 

        ///<summary>
        ///The provider-specific data type name.
        ///</summary>
        public string DbTypeName { get; set; }

        /// <summary>
        /// Maximum length if applicable
        /// </summary>
        public int? MaxLength { get; set; }
        /// <summary>
        /// For numeric types, precision is the number of digits in a number. For example, the number 123.45 has a precision of 5 and a scale of 2.
        /// </summary>
        public int? Precision { get; set; }
        /// <summary>
        /// For numeric types, scale is the number of digits to the right of the decimal point in a number. For example, the number 123.45 has a precision of 5 and a scale of 2.
        /// </summary>
        public int? Scale { get; set; }
        /// <summary>
        /// Is Nullable
        /// </summary>
        public bool? Nullable { get; set; }
        /// <summary>
        /// Default value
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// DataType based on <see cref="DbTypeName"/> (after calling <see cref="DatabaseSchemaFixer.UpdateDataTypes(DatabaseSchema)"/>)
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}