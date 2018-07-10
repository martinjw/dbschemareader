using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    public class ServiceCollectionExtensionsWriter
    {
        private ClassBuilder classBuilder;
        private CodeWriterSettings codeWriterSettings { get; }
        private DatabaseSchema schema { get; }
        private string className = "ServiceCollectionExtensions";

        public ServiceCollectionExtensionsWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings)
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
                    using (classBuilder.BeginNest($"public static IServiceCollection AddEnterpriseDataRepositories(this IServiceCollection services)"))
                    {
                        foreach (var t in schema.Tables)
                        {
                            var interfaceName = CodeWriterUtils.GetRepositoryInterfaceName(t);
                            var implementationName = CodeWriterUtils.GetRepositoryImplementationName(t);
                            classBuilder.AppendLine($"services.AddTransient<{interfaceName}, {implementationName}>();");
                        }

                        classBuilder.AppendLine("return services;");
                    }
                }
            }

            return classBuilder.ToString();
        }

        private void WriteUsings()
        {
            classBuilder.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            foreach (var u in codeWriterSettings.Usings)
            {
                classBuilder.AppendLine($"using {u};");
            }

            classBuilder.AppendLine("");
        }
    }
}
