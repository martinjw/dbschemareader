using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    public class EnumeratedDataTypeWriter
    {
        private ClassBuilder classBuilder;
        private CodeWriterSettings codeWriterSettings { get; }
        private DatabaseSchema schema { get; }

        public EnumeratedDataTypeWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings)
        {
            this.codeWriterSettings = codeWriterSettings;
            this.schema = schema;
        }

        public void Execute()
        {
            foreach (var type in schema.DataTypes)
            {
                classBuilder = new ClassBuilder();
                if (!(type is EnumeratedDataType))
                {
                    continue;
                }

                var implementationText = Write(type as EnumeratedDataType);
                if (string.IsNullOrEmpty(implementationText))
                {
                    continue;
                }

                CodeWriterUtils.WriteClassFile(codeWriterSettings.OutputDirectory, type.NetDataType, implementationText);
            }
        }

        private string Write(EnumeratedDataType type)
        {
            CodeWriterUtils.WriteFileHeader(classBuilder);
            WriteUsings();
            classBuilder.BeginNest($"namespace {codeWriterSettings.Namespace}");
            classBuilder.BeginNest($"public enum {type.NetDataType}");
            WriteEnumerationValues(type);
            classBuilder.EndNest();
            classBuilder.EndNest();
            return classBuilder.ToString();
        }

        private void WriteUsings()
        {
            classBuilder.AppendLine("using NpgsqlTypes;");
            classBuilder.AppendLine("");
        }

        private void WriteEnumerationValues(EnumeratedDataType type)
        {
            for (var i = 0; i < type.EnumerationValues.Count; i++)
            {
                var enumerationValueToWrite = type.EnumerationValues[i].Replace(" ", "_");
                if (i < type.EnumerationValues.Count - 1)
                {
                    enumerationValueToWrite += ",";
                }
                classBuilder.AppendLine($"[PgName(\"{type.EnumerationValues[i]}\")]");
                classBuilder.AppendLine(enumerationValueToWrite);
            }
        }
    }
}
