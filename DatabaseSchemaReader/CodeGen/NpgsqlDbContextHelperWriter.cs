using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    public class NpgsqlDbContextHelperWriter
    {
        private ClassBuilder classBuilder;
        private CodeWriterSettings codeWriterSettings { get; }
        private DatabaseSchema schema { get; }

        private string className = "NpgsqlDbContextHelper";

        public NpgsqlDbContextHelperWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings)
        {
            this.codeWriterSettings = codeWriterSettings;
            this.schema = schema;
            classBuilder = new ClassBuilder();
        }

        public void Execute()
        {
            var implementationText = Write();
            CodeWriterUtils.WriteClassFile(codeWriterSettings.OutputDirectory, className, implementationText);
        }

        private string Write()
        {
            WriteUsings();
            using (classBuilder.BeginNest($"namespace {codeWriterSettings.Namespace}"))
            {
                using (classBuilder.BeginNest($"public static class {className}"))
                {
                    using (classBuilder.BeginNest("public static void RegisterEnumerationTypeMappings()"))
                    {
                        WriteMapEnumerations();
                        classBuilder.AppendLine("NpgsqlConnection.GlobalTypeMapper.UseNetTopologySuite();");
                    }
                }
            }

            return classBuilder.ToString();
        }

        private void WriteMapEnumerations()
        {
            foreach (var type in schema.DataTypes)
            {
                if (!(type is EnumeratedDataType))
                {
                    continue;
                }

                classBuilder.AppendLine($"NpgsqlConnection.GlobalTypeMapper.MapEnum<{type.NetDataType}>(\"{type.NetDataType}\");");
            }

            classBuilder.AppendLine("");
        }

        private void WriteUsings()
        {
            classBuilder.AppendLine("using Npgsql;");
            foreach (var u in codeWriterSettings.Usings.Where(u => !u.Equals(codeWriterSettings.Namespace)))
            {
                classBuilder.AppendLine($"using {u};");
            }

            classBuilder.AppendLine("");
        }
    }
}
