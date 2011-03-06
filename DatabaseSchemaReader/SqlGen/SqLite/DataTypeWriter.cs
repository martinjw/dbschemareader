using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.SqlGen.SqLite
{
    static class DataTypeWriter
    {
        public static string SqLiteDataType(DatabaseColumn column)
        {
            //sqlite is not strongly typed, and the type affinities (http://www.sqlite.org/datatype3.html) are very limited
            // (text, integer, real, blob)
            //the ado provider uses the column types for richer support
            //ado mapping http://sqlite.phxsoftware.com/forums/t/31.aspx

            var dataType = column.DbDataType.ToUpperInvariant();
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
