using DatabaseSchemaReader.DataSchema;
using System;
using System.Linq;

namespace DatabaseSchemaReader.CodeGen
{
    public class RepositoryInterfaceWriter
    {
        private readonly DatabaseTable table;
        private readonly ClassBuilder classBuilder;
        private DataAnnotationWriter _dataAnnotationWriter;
        private readonly CodeWriterSettings codeWriterSettings;

        public RepositoryInterfaceWriter(DatabaseTable table, CodeWriterSettings codeWriterSettings)
        {
            this.codeWriterSettings = codeWriterSettings;
            this.table = table;
            classBuilder = new ClassBuilder();
        }

        public string Write()
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
            var interfaceDefinition = $"public interface {CodeWriterUtils.GetRepositoryInterfaceName(table)}";
            classBuilder.AppendXmlSummary(comment);
            classBuilder.BeginNest(interfaceDefinition, comment);
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
            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, false))};");

            var methodParametersByCustomer = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, true);
            if (methodParametersByCustomer == null || !methodParametersByCustomer.Any())
            {
                return;
            }

            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, methodParametersByCustomer)};");
        }

        private void WriteUpdates()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings, false);
            methodParameters = CodeWriterUtils.AddEntityParameter(methodParameters, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, methodParameters)};");

            var methodParametersByCustomer = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings, true);
            if (methodParametersByCustomer == null || !methodParametersByCustomer.Any())
            {
                return;
            }

            methodParametersByCustomer = CodeWriterUtils.AddEntityParameter(methodParametersByCustomer, table, "An entity with updated values.");
            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, methodParametersByCustomer)};");
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
            var methodParametersUniqueByCustomer = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, true, true);
            if (methodParametersUniqueByCustomer != null && methodParametersUniqueByCustomer.Any())
            {
                classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParametersUniqueByCustomer)};");
            }
        }

        private void WriteGetUnique()
        {
            var methodParametersUnique = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, false, true);
            if (methodParametersUnique != null && methodParametersUnique.Any())
            {
                classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParametersUnique)};");
            }
        }

        private void WriteGetByCustomer()
        {
            var methodParametersByCustomer = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, true, false);
            if (methodParametersByCustomer != null && methodParametersByCustomer.Any())
            {
                classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParametersByCustomer)};");
            }
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
            classBuilder.AppendLine("");
        }
    }
}
