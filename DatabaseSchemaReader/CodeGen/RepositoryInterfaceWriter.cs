using DatabaseSchemaReader.DataSchema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseSchemaReader.CodeGen
{
    public class RepositoryInterfaceWriter
    {
        private DatabaseTable table;
        private ClassBuilder classBuilder;
        private IEnumerable<string> logicalDeleteColumns;
        private CodeWriterSettings codeWriterSettings { get; }
        private DatabaseSchema schema { get; }

        public RepositoryInterfaceWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings)
        {
            this.codeWriterSettings = codeWriterSettings;
            this.schema = schema;
        }

        public void Execute()
        {
            foreach (var t in schema.Tables)
            {
                table = t;
                classBuilder = new ClassBuilder();
                var implementationText = Write();
                CodeWriterUtils.WriteClassFile(codeWriterSettings.OutputDirectory, CodeWriterUtils.GetRepositoryInterfaceName(table), implementationText);
            }
        }

        private string Write()
        {
            if (string.IsNullOrEmpty(table.NetName) && table.DatabaseSchema != null)
            {
                PrepareSchemaNames.Prepare(table.DatabaseSchema, codeWriterSettings.Namer);
            }

            CodeWriterUtils.WriteFileHeader(classBuilder);
            WriteUsings();
            CodeWriterUtils.BeginNestNamespace(classBuilder, codeWriterSettings);
            var tableOrView = table is DatabaseView ? "view" : "table";
            var comment = $"Interface providing repository CRUD operations for the {table.Name} {tableOrView}";
            var interfaceDefinition = $"public partial interface {CodeWriterUtils.GetRepositoryInterfaceName(table)}";
            classBuilder.AppendXmlSummary(comment);
            classBuilder.BeginNest(interfaceDefinition);
            WriteInterfaceMembers();
            classBuilder.EndNest(); // interface
            classBuilder.EndNest(); // namespace
            return classBuilder.ToString();
        }

        private void WriteInterfaceMembers()
        {
            WriteCreate();
            WriteGets();
            WriteGetLists();
            WriteGetListBys();
            WriteUpdates();
            WriteDeletes();
        }

        private void WriteDeletes()
        {
            WriteDelete();
            WriteDeleteByCustomer();
            WriteDeleteUnique();
            WriteDeleteUniqueByCustomer();
        }

        private void WriteDelete()
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, false, false);
            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteDeleteByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, true, false);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteDeleteUnique()
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, false, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteDeleteUniqueByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, true, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteUpdates()
        {
            WriteUpdate();
            WriteUpdateByCustomer();
            WriteUpdateUnique();
            WriteUpdateUniqueByCustomer();
        }

        private void WriteUpdate()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings, false, false);
            methodParameters = CodeWriterUtils.AddEntityParameter(methodParameters, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteUpdateByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings, true, false);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            methodParameters = CodeWriterUtils.AddEntityParameter(methodParameters, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteUpdateUnique()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings, false, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            methodParameters = CodeWriterUtils.AddEntityParameter(methodParameters, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteUpdateUniqueByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings, true, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            methodParameters = CodeWriterUtils.AddEntityParameter(methodParameters, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteGetListBys()
        {
            var combinations = CodeWriterUtils.GetGetListByColumnCombinations(table)?.ToList();
            combinations?.ForEach(c => classBuilder.AppendLine($"{CodeWriterUtils.GetGetListByMethodSignature(table, c, codeWriterSettings, CodeWriterUtils.GetMethodParametersForColumns(c, codeWriterSettings))};"));
        }

        private void WriteGetLists()
        {
            classBuilder.AppendLine($"{CodeWriterUtils.GetGetListMethodSignature(table, codeWriterSettings, CodeWriterUtils.GetGetListMethodParameters(table, codeWriterSettings, false))};");
            var methodParametersByCustomer = CodeWriterUtils.GetGetListMethodParameters(table, codeWriterSettings, true);
            if (methodParametersByCustomer == null || !methodParametersByCustomer.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetGetListMethodSignature(table, codeWriterSettings, methodParametersByCustomer)};");
        }

        private void WriteGets()
        {
            WriteGet();
            WriteGetByCustomer();
            WriteGetUnique();
            WriteGetUniqueByCustomer();
        }

        private void WriteGetUniqueByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, true, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteGetUnique()
        {
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, false, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteGetByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, true, false);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteGet()
        {
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, false, false);
            classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParameters)};");
        }

        private void WriteCreate()
        {
            classBuilder.AppendLine($"{CodeWriterUtils.GetCreateMethodSignature(table, CodeWriterUtils.GetCreateMethodParameters(table))};");
        }

        private void WriteUsings()
        {
            classBuilder.AppendLine("using System;");
            classBuilder.AppendLine("using System.Collections.Generic;");
            foreach (var u in codeWriterSettings.Usings)
            {
                classBuilder.AppendLine($"using {u};");
            }

            classBuilder.AppendLine("");
        }
    }
}
