using System;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// An argument (in or out parameter) to a stored procedure or function.
    /// </summary>
    [Serializable]
    public class DatabaseArgument
    {
        public DatabaseSchema DatabaseSchema { get; set; }
        public string Name { get; set; }
        public string SchemaOwner { get; set; }

        public string ProcedureName { get; set; }
        public string PackageName { get; set; }

        /// <summary>
        /// Gets or sets the dataType (string format).
        /// </summary>
        /// <value>The dataType.</value>
        public string DatabaseDataType { get; set; }

        /// <summary>
        /// Gets or sets the dataType. MAY BE NULL (eg Oracle REF CURSOR is not in datatypes list) - in which case refer to the string <see cref="DatabaseDataType"/>.
        /// </summary>
        /// <value>The dataType.</value>
        public DataType DataType { get; set; }
        public decimal Ordinal { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public int? Length { get; set; }
        public bool In { get; set; }
        public bool Out { get; set; }
        public override string ToString()
        {
            return Name;
        }

    }
}
