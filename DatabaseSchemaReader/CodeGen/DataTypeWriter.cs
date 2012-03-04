using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// Converts column datatypes into string representations of .Net types
    /// </summary>
    class DataTypeWriter
    {
        /// <summary>
        /// Gets or sets the code target.
        /// </summary>
        /// <value>
        /// The code target.
        /// </value>
        public CodeTarget CodeTarget { get; set; }

        public string Write(DatabaseColumn column)
        {
            var dataType = FindDataType(column);
            return dataType;
        }

        private string FindDataType(DatabaseColumn column)
        {
            var dt = column.DataType;
            string dataType;
            if (dt == null)
            {
                dataType = "object";
            }
            else if (CodeTarget == CodeTarget.PocoEntityCodeFirst || CodeTarget == CodeTarget.PocoRiaServices)
            {
                //EF needs the default mapping type
                dataType = dt.NetDataTypeCSharpName;
            }
            else
            {
                //use precision and scale for more precise conversion
                dataType = dt.NetCodeName(column);
            }
            //if it's nullable (and not string or array)
            if (column.Nullable &&
                dt != null &&
                !dt.IsString &&
                !dataType.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
            {
                dataType += "?"; //nullable
            }
            return dataType;
        }
    }
}
