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
                if (CodeTarget == CodeTarget.PocoEntityCodeFirst)
                {
                    //TODO: for EF 6 these will be System.Data.Entity.Spatial.Db*
                    //Change after EF 6 RTM for next DSR release

                    //spatial types for EF 5 supported by SQLServer and Oracle/Devart
                    if ("geometry".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase) ||
                        "SDO_GEOMETRY".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase))
                    {
                        return "System.Data.Spatial.DbGeometry";
                    }
                    if ("geography".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase) ||
                        "SDO_GEOGRAPHY".Equals(column.DbDataType, StringComparison.OrdinalIgnoreCase))
                    {
                        return "System.Data.Spatial.DbGeography";
                    }
                }
                dataType = "object";
            }
            else if (CodeTarget == CodeTarget.PocoEntityCodeFirst || CodeTarget == CodeTarget.PocoRiaServices)
            {
                //EF needs the default mapping type
                dataType = dt.NetCodeName(column);
                //dataType = dt.NetDataTypeCSharpName;
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
                !string.IsNullOrEmpty(dataType) &&
                !dataType.EndsWith("[]", StringComparison.OrdinalIgnoreCase))
            {
                dataType += "?"; //nullable
            }
            return dataType;
        }
    }
}
