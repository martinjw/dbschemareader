using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    public class DataTypeWriter
    {
        private readonly DataType _dataType;
        private readonly ClassBuilder _cb;
        private readonly CodeWriterSettings _codeWriterSettings;

        public DataTypeWriter(DataType dataType, CodeWriterSettings codeWriterSettings)
        {
            _codeWriterSettings = codeWriterSettings;
            _dataType = dataType;
            _cb = new ClassBuilder();
        }

        public string Write()
        {
            return _dataType.WriteCodeFile(_codeWriterSettings, _cb);
        }

        public static string FindDataType(DatabaseColumn column)
        {
            var dt = column.DataType;
            string dataType;
            if (dt == null)
            {
                dataType = "object";
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
                !dataType.EndsWith("[]", StringComparison.OrdinalIgnoreCase) &&
                !dt.IsGeospatial)
            {
                dataType += "?"; //nullable
            }
            return dataType;
        }
    }
}
