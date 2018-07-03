using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    public class DataTypeWriter
    {
        private ClassBuilder classBuilder;
        private DataType dataType;
        public CodeWriterSettings CodeWriterSettings { get; }
        public DatabaseSchema Schema { get; }

        public DataTypeWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings)
        {
            CodeWriterSettings = codeWriterSettings;
            Schema = schema;
        }

        public void Execute()
        {

            foreach (var type in Schema.DataTypes)
            {
                dataType = type;
                classBuilder = new ClassBuilder();
                var txt = Write();
                if (string.IsNullOrEmpty(txt))
                {
                    continue;
                }

                CodeWriterUtils.WriteClassFile(CodeWriterSettings.OutputDirectory, type.NetDataType, txt);
            }
        }
        
        public string Write()
        {
            return dataType.WriteCodeFile(CodeWriterSettings, classBuilder);
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
