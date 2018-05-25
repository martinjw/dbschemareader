using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

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
            classBuilder.AppendLine($"{CodeWriterUtils.GetCreateMethodSignature(table, CodeWriterUtils.GetCreateMethodParameters(table))};");
            classBuilder.AppendLine($"{CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings))};");
            classBuilder.AppendLine($"{CodeWriterUtils.GetGetListMethodSignature(table, codeWriterSettings, CodeWriterUtils.GetGetListMethodParameters(table, codeWriterSettings))};");
            var combinations = CodeWriterUtils.GetGetListByColumnCombinations(table)?.ToList();
            var primaryKeyColumns = CodeWriterUtils.GetPrimaryKeyColumns(table);
            if (combinations != null)
            {
                foreach (var c in combinations)
                {
                    if (c.SequenceEqual(primaryKeyColumns))
                    {
                        continue;
                    }

                    classBuilder.AppendLine($"{CodeWriterUtils.GetGetListByMethodSignature(table, c, codeWriterSettings, CodeWriterUtils.GetMethodParametersForColumns(c, codeWriterSettings))};");
                }
            }

            /*foreach (var foreignKey in table.ForeignKeys)
            {
                classBuilder.AppendLine($"{CodeWriterUtils.GetWithMethodSignature(table, foreignKey, codeWriterSettings)};");
            }

            foreach (var foreignKeyChild in table.ForeignKeyChildren)
            {
                foreach (var foreignForeignKey in CodeWriterUtils.GetWithForeignKeys(table, foreignKeyChild))
                {
                    classBuilder.AppendLine($"{CodeWriterUtils.GetWithMethodSignature(table, foreignKeyChild, foreignForeignKey, codeWriterSettings)};");
                }
            }*/

            classBuilder.AppendLine($"{CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings))};");
            classBuilder.AppendLine($"{CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings))};");
        }

        private void WriteUsings()
        {
            classBuilder.AppendLine("using System;");
            classBuilder.AppendLine("using System.Collections.Generic;");
            classBuilder.AppendLine("");
        }
    }
}
