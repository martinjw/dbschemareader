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
        public CodeWriterSettings CodeWriterSettings { get; }
        public DatabaseSchema Schema { get; }

        public RepositoryInterfaceWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings)
        {
            CodeWriterSettings = codeWriterSettings;
            Schema = schema;
        }

        public void Execute()
        {
            foreach (var t in Schema.Tables)
            {
                table = t;
                classBuilder = new ClassBuilder();
                var implementationText = Write();
                CodeWriterUtils.WriteClassFile(CodeWriterSettings.OutputDirectory, CodeWriterUtils.GetRepositoryInterfaceName(table), implementationText);
            }
        }

        private string Write()
        {
            if (string.IsNullOrEmpty(table.NetName) && table.DatabaseSchema != null)
            {
                PrepareSchemaNames.Prepare(table.DatabaseSchema, CodeWriterSettings.Namer);
            }

            CodeWriterUtils.WriteFileHeader(classBuilder);
            WriteUsings();
            CodeWriterUtils.BeginNestNamespace(classBuilder, CodeWriterSettings);
            var tableOrView = table is DatabaseView ? "view" : "table";
            var comment = $"Interface providing repository CRUD operations for the {table.Name} {tableOrView}";
            var interfaceDefinition = $"public interface {CodeWriterUtils.GetRepositoryInterfaceName(table)}";
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
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, CodeWriterSettings, false, false);
            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteDeleteByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, CodeWriterSettings, true, false);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteDeleteUnique()
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, CodeWriterSettings, false, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteDeleteUniqueByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, CodeWriterSettings, true, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, CodeWriterSettings, methodParameters)};");
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
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, CodeWriterSettings, false, false);
            methodParameters = CodeWriterUtils.AddEntityParameter(methodParameters, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteUpdateByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, CodeWriterSettings, true, false);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            methodParameters = CodeWriterUtils.AddEntityParameter(methodParameters, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteUpdateUnique()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, CodeWriterSettings, false, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            methodParameters = CodeWriterUtils.AddEntityParameter(methodParameters, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteUpdateUniqueByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, CodeWriterSettings, true, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            methodParameters = CodeWriterUtils.AddEntityParameter(methodParameters, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteGetListBys()
        {
            var combinations = CodeWriterUtils.GetGetListByColumnCombinations(table)?.ToList();
            combinations?.ForEach(c => classBuilder.AppendLine($"{CodeWriterUtils.GetGetListByMethodSignature(table, c, CodeWriterSettings, CodeWriterUtils.GetMethodParametersForColumns(c, CodeWriterSettings))};"));
        }

        private void WriteGetLists()
        {
            classBuilder.AppendLine($"{CodeWriterUtils.GetGetListMethodSignature(table, CodeWriterSettings, CodeWriterUtils.GetGetListMethodParameters(table, CodeWriterSettings, false))};");
            var methodParametersByCustomer = CodeWriterUtils.GetGetListMethodParameters(table, CodeWriterSettings, true);
            if (methodParametersByCustomer == null || !methodParametersByCustomer.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetGetListMethodSignature(table, CodeWriterSettings, methodParametersByCustomer)};");
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
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, CodeWriterSettings, true, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteGetUnique()
        {
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, CodeWriterSettings, false, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteGetByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, CodeWriterSettings, true, false);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteGet()
        {
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, CodeWriterSettings, false, false);
            classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, CodeWriterSettings, methodParameters)};");
        }

        private void WriteCreate()
        {
            classBuilder.AppendLine($"{CodeWriterUtils.GetCreateMethodSignature(table, CodeWriterUtils.GetCreateMethodParameters(table))};");
        }

        private void WriteUsings()
        {
            classBuilder.AppendLine("using System;");
            classBuilder.AppendLine("using System.Collections.Generic;");
            classBuilder.AppendLine("");
        }
    }
}
