using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqLite
{
    class DataTypeWriter : IDataTypeWriter
    {
        public string WriteDataType(DatabaseColumn column)
        {
            //sqlite is not strongly typed, and the type affinities (http://www.sqlite.org/datatype3.html) are very limited
            // (text, integer, real, blob)
            //the ado provider uses the column types for richer support
            //ado mapping http://sqlite.phxsoftware.com/forums/t/31.aspx

            if (column == null) return string.Empty;
            var dt = column.DataType;
            if (dt != null)
            {
                if (dt.IsString)
                    return "TEXT";
                if (dt.IsInt)
                    return "INTEGER";
                if (dt.IsNumeric)
                    return "NUMERIC"; //Integer or Real
                if (dt.IsFloat)
                    return "REAL";
            }
            if (string.IsNullOrEmpty(column.DbDataType)) return string.Empty;
            var dataType = column.DbDataTypeStandard();

            if (dataType == "IMAGE" || dataType.IndexOf("BINARY", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return "BLOB";
            }
            if (dataType == "BIT")
            {
                return "INTEGER"; // boolean = 0 or 1
            }
            if (dataType == "DATE" || dataType == "DATETIME")
            {
                //a hint to the ado provider
                return "DATETIME";
            }


            return dataType;
        }

    }
}
