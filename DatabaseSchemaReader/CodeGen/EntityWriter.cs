using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    public class EntityWriter
    {
        private readonly MappingNamer mappingNamer;
        private ClassBuilder classBuilder;
        private DatabaseTable table;
        private DataAnnotationWriter dataAnnotationWriter;
        public CodeWriterSettings CodeWriterSettings { get; }
        public DatabaseSchema Schema { get; }

        public EntityWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings)
        {
            Schema = schema;
            CodeWriterSettings = codeWriterSettings;
            PrepareSchemaNames.Prepare(schema, CodeWriterSettings.Namer);
            dataAnnotationWriter = new DataAnnotationWriter(false, codeWriterSettings);
            mappingNamer = new MappingNamer();
        }

        public void Execute()
        {
            foreach (var t in Schema.Tables)
            {
                table = t;
                classBuilder = new ClassBuilder();
                var implementationText = Write();
                CodeWriterUtils.WriteClassFile(CodeWriterSettings.OutputDirectory, table.NetName, implementationText);
            }
        }

        public string Write()
        {
            if (string.IsNullOrEmpty(table.NetName) && table.DatabaseSchema != null)
            {
                PrepareSchemaNames.Prepare(table.DatabaseSchema, CodeWriterSettings.Namer);
            }

            CodeWriterUtils.WriteFileHeader(classBuilder);
            WriteUsings();
            CodeWriterUtils.BeginNestNamespace(classBuilder, CodeWriterSettings);

            classBuilder.AppendXmlSummary($"Class representing the {table.Name} table.");
            classBuilder.AppendLine($"[Table(\"\\\"{table.Name}\\\"\")]");
            using (classBuilder.BeginNest($"public class {table.NetName}"))
            {
                WriteAllMembers();
            }
            
            classBuilder.EndNest();
            return classBuilder.ToString();
        }

        private void WriteAllMembers()
        {
            classBuilder.AppendLine("public IDbContext DbContext { get; set; }");
            classBuilder.AppendLine("");
            WritePrimaryKeyColumnProperties();
            WriteNonPrimaryKeyColumnProperties();
            WriteForeignKeyProperties();
            WriteForeignKeyCollectionProperties();
            WriteWiths();
        }

        private void WriteForeignKeyProperties()
        {
            foreach (var foreignKey in table.ForeignKeys)
            {
                WriteForeignKey(foreignKey);
            }
        }

        private void WriteNonPrimaryKeyColumnProperties()
        {
            foreach (var column in table.Columns.Where(c => !c.IsPrimaryKey))
            {
                WriteColumn(column);
            }
        }

        private void WriteWiths()
        {
            foreach (var foreignKey in table.ForeignKeys)
            {
                WriteWith(foreignKey);
            }

            foreach (var foreignKey in table.ForeignKeyChildren)
            {
                WriteWith(foreignKey);
            }
        }

        public void WriteWith(DatabaseTable foreignKeyChild)
        {
            var ffks = CodeWriterUtils.GetWithForeignKeys(table, foreignKeyChild).ToList();
            foreach (var ffk in ffks)
            {
                var ffkTable = table.DatabaseSchema.FindTableByName(ffk.TableName);
                var ffkReferencedTable = ffk.ReferencedTable(table.DatabaseSchema);
                var ffkColumns = ffk.Columns.Select(item => ffkTable.FindColumn(item));
                ffkColumns.OrderBy(item => item.Name);
                var ffkReferencedColumns = ffk.ReferencedColumns(table.DatabaseSchema).Select(item => ffkReferencedTable.FindColumn(item));

                var withMethodSignature = CodeWriterUtils.GetWithMethodSignature(
                    ffkReferencedTable,
                    ffkTable,
                    ffk,
                    CodeWriterSettings);

                var propertyName = CodeWriterSettings.Namer.ForeignKeyCollectionName(ffkReferencedTable.Name, ffkTable, ffk);
                var repositoryNameForFfkTable = CodeWriterUtils.GetRepositoryImplementationName(foreignKeyChild);
                var repositoryMethodNameForFfkTable = CodeWriterUtils.GetGetMethodName(ffkColumns, CodeWriterSettings, false);
                classBuilder.BeginNest($"public {withMethodSignature}");
                var repositoryMethodCallParametersForFfkTable = new List<string> { "DbContext" };
                foreach (var ffkReferencedColumn in ffkReferencedColumns)
                {
                    var parameter = $"{CodeWriterUtils.GetPropertyNameForDatabaseColumn(ffkReferencedColumn)}";
                    if (ffkReferencedColumn.Nullable && DataTypeWriter.FindDataType(ffkReferencedColumn).EndsWith("?"))
                    {
                        using (classBuilder.BeginNest($"if (!{parameter}.HasValue)"))
                        {
                            classBuilder.AppendLine($"{propertyName} = new List<{ffkTable.NetName}>();");
                            classBuilder.AppendLine("return this;");
                        }

                        classBuilder.AppendLine("");
                        parameter += ".Value";
                    }

                    repositoryMethodCallParametersForFfkTable.Add(parameter);
                }

                var repositoryMethodCallParametersForFfkTablePrinted = string.Join(", ", repositoryMethodCallParametersForFfkTable);
                classBuilder.AppendLine($"{propertyName} = {repositoryNameForFfkTable}.{repositoryMethodNameForFfkTable}({repositoryMethodCallParametersForFfkTablePrinted});");
                classBuilder.AppendLine("return this;");
                classBuilder.EndNest();
                classBuilder.AppendLine("");
            }
        }

        public void WriteWith(DatabaseConstraint foreignKey)
        {
            // TODO: refactor this method to be consistent with approach taken for other overload
            var refTable = foreignKey.ReferencedTable(table.DatabaseSchema);
            var dataType = refTable.NetName;

            if (foreignKey.Columns.Count != foreignKey.ReferencedColumns(table.DatabaseSchema).Count())
            {
                throw new InvalidOperationException("Number of foreign key columns does not match number of columns referenced!");
            }

            classBuilder.BeginNest($"public {CodeWriterUtils.GetWithMethodSignature(table, foreignKey, CodeWriterSettings)}");

            var methodCallParameters = new List<string>
            {
                "DbContext"
            };

            var propertyName = CodeWriterSettings.Namer.ForeignKeyName(table, foreignKey);
            foreach (var fkc in foreignKey.Columns)
            {
                var tc = table.Columns.Single(_tc => _tc.Name == fkc);
                var parameter = $"{CodeWriterUtils.GetPropertyNameForDatabaseColumn(tc)}";
                if (tc.Nullable && DataTypeWriter.FindDataType(tc).EndsWith("?")) // KE: need the check for the "?" so that we correctly handle reference types like string
                {
                    using (classBuilder.BeginNest($"if (!{parameter}.HasValue)"))
                    {
                        classBuilder.AppendLine($"{propertyName} = null;");
                        classBuilder.AppendLine("return this;");
                    }

                    classBuilder.AppendLine("");
                    parameter += ".Value";
                }

                methodCallParameters.Add(parameter);
            }

            var s = string.Join(", ", methodCallParameters);
            var referencedColumnNames = foreignKey.ReferencedColumns(table.DatabaseSchema).ToList();
            referencedColumnNames.Sort();
            var referencedColumns = referencedColumnNames.Select(c => foreignKey.ReferencedTable(table.DatabaseSchema).FindColumn(c));
            var methodParameters = CodeWriterUtils.GetMethodParametersForColumns(referencedColumns, CodeWriterSettings);
            var methodName = CodeWriterUtils.GetMethodName(methodParameters, CodeWriterSettings, true, CodeWriterUtils.BaseMethodNameGet);
            classBuilder.AppendLine($"{propertyName} = {CodeWriterUtils.GetRepositoryImplementationName(foreignKey.ReferencedTable(table.DatabaseSchema))}.{methodName}({s});");
            classBuilder.AppendLine("return this;");
            classBuilder.EndNest();
            classBuilder.AppendLine("");
        }

        private void WritePrimaryKeyColumnProperties()
        {
            foreach (var column in table.Columns.Where(c => c.IsPrimaryKey))
            {
                WriteColumn(column, false);
            }
        }

        private void WriteUsings()
        {
            classBuilder.AppendLine("using System;");
            classBuilder.AppendLine("using System.Collections.Generic;");
            classBuilder.AppendLine("using System.ComponentModel.DataAnnotations;");
            classBuilder.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            if (table.Columns.Select(c => c.DataType.IsGeospatial).Contains(true))
            {
                classBuilder.AppendLine("using NetTopologySuite.Geometries;");
            }

            classBuilder.AppendLine("using PeopleNet.EnterpriseData.DataAccess.Repositories;");
            classBuilder.AppendLine("");
        }

        private void WriteForeignKeyCollectionProperties()
        {
            var listType = "IEnumerable<";

            var hasTablePerTypeInheritance =
                (table.ForeignKeyChildren.Count(fk => table.IsSharedPrimaryKey(fk)) > 1);

            foreach (var foreignKey in table.ForeignKeyChildren)
            {
                if (foreignKey.IsManyToManyTable() && CodeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
                {
                    WriteManyToManyCollection(foreignKey);
                    continue;
                }
                if (table.IsSharedPrimaryKey(foreignKey))
                {
                    if (hasTablePerTypeInheritance)
                        continue;
                    //type and property name are the same
                    classBuilder.AppendAutomaticProperty(foreignKey.NetName, foreignKey.NetName, true);
                    continue;
                }

                //the other table may have more than one fk pointing at this table
                var fks = table.InverseForeignKeys(foreignKey);
                foreach (var fk in fks)
                {
                    var propertyName = CodeWriterSettings.Namer.ForeignKeyCollectionName(table.Name, foreignKey, fk);
                    var dataType = listType + foreignKey.NetName + ">";
                    WriteForeignKeyChild(propertyName, dataType);
                    classBuilder.AppendLine("");
                }
            }
        }

        private void WriteForeignKeyChild(string propertyName, string dataType)
        {
            if (CodeWriterSettings.CodeTarget == CodeTarget.PocoRiaServices)
                classBuilder.AppendLine("[Include]");
            classBuilder.AppendAutomaticCollectionProperty(dataType, propertyName, false);
        }

        private void WriteManyToManyCollection(DatabaseTable foreignKey)
        {
            //look over the junction table to find the other many-to-many end
            var target = foreignKey.ManyToManyTraversal(table);
            if (target == null)
            {
                Debug.WriteLine("Can't navigate the many to many relationship for " + table.Name + " to " + foreignKey.Name);
                return;
            }
            var propertyName = CodeWriterSettings.Namer.NameCollection(target.NetName);
            var dataType = "ICollection<" + target.NetName + ">";
            classBuilder.AppendAutomaticCollectionProperty(dataType, propertyName, false);

        }

        private void WriteManyToManyInitialize(DatabaseTable foreignKey)
        {
            //look over the junction table to find the other many-to-many end
            var target = foreignKey.ManyToManyTraversal(table);
            if (target == null)
            {
                return;
            }
            var propertyName = CodeWriterSettings.Namer.NameCollection(target.NetName);
            var dataType = "List<" + target.NetName + ">";
            classBuilder.AppendLine(propertyName + " = new " + dataType + "();");
        }

        private void InitializeCollectionsInConstructor()
        {
            if (!table.ForeignKeyChildren.Any()) return;
            using (classBuilder.BeginNest("public " + table.NetName + "()"))
            {
                foreach (var foreignKey in table.ForeignKeyChildren)
                {
                    if (foreignKey.IsManyToManyTable() && CodeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
                    {
                        WriteManyToManyInitialize(foreignKey);
                        continue;
                    }
                    if (table.IsSharedPrimaryKey(foreignKey))
                    {
                        continue;
                    }
                    var fks = table.InverseForeignKeys(foreignKey);
                    foreach (DatabaseConstraint fk in fks)
                    {
                        var propertyName = CodeWriterSettings.Namer.ForeignKeyCollectionName(table.Name, foreignKey, fk);
                        var dataType = "List<" + foreignKey.NetName + ">";
                        classBuilder.AppendLine(propertyName + " = new " + dataType + "();");
                    }
                }
            }
            classBuilder.AppendLine("");
        }

        private void WriteColumn(DatabaseColumn column)
        {
            WriteColumn(column, false);
        }

        private void WriteColumn(DatabaseColumn column, bool notNetName)
        {
            var propertyName = CodeWriterUtils.GetPropertyNameForDatabaseColumn(column);
            var dataType = DataTypeWriter.FindDataType(column);

            if (notNetName)
            {
                //in EF, you want a fk Id property
                //must not conflict with entity fk name
                propertyName += "Id";
            }

            CodeWriterSettings.CodeInserter.WriteColumnAnnotations(table, column, classBuilder);
            dataAnnotationWriter.Write(classBuilder, column, propertyName);
            var useVirtual = true;
            classBuilder.AppendAutomaticProperty(dataType, propertyName, useVirtual);
        }

        private static DatabaseTable FindForeignKeyTable(DatabaseColumn column)
        {
            var refTable = column.ForeignKeyTable;
            if (refTable != null) return refTable;
            //column may be in multiple fks, and the model may be broken
            var table = column.Table;
            if (table == null) return null;
            //find the first foreign key containing this column
            var fk = table.ForeignKeys.FirstOrDefault(c => c.Columns.Contains(column.Name));
            if (fk != null)
            {
                refTable = fk.ReferencedTable(table.DatabaseSchema);
            }
            return refTable;
        }

        private void WriteForeignKey(DatabaseConstraint foreignKey)
        {
            // get the reference table
            var refTable = foreignKey.ReferencedTable(table.DatabaseSchema);

            if (refTable == null)
            {
                //we can't find the foreign key table, so just write the columns
                WriteForeignKeyProperties(foreignKey, "");
                return;
            }

            var propertyName = CodeWriterSettings.Namer.ForeignKeyName(table, foreignKey);
            var dataType = refTable.NetName;

            classBuilder.AppendAutomaticProperty(dataType, propertyName);

            if (false && CodeWriterSettings.UseForeignKeyIdProperties)
            {
                WriteForeignKeyProperties(foreignKey, propertyName);
            }
        }

        private void WriteForeignKeyProperties(DatabaseConstraint foreignKey, string propertyName)
        {
            //for code first, we may have to write scalar properties
            //1 if the fk is also a pk
            //2 if they selected use Foreign Key Ids
            foreach (var columnName in foreignKey.Columns)
            {
                var column = table.FindColumn(columnName);
                if (column == null) continue;
                //primary keys are already been written
                if (!column.IsPrimaryKey)
                {
                    WriteColumn(column, propertyName.Equals(column.NetName));
                }
            }
        }

        private void UpdateEntityNames(string className, string tableName)
        {
            if (mappingNamer.EntityNames.Contains(className))
            {
                Debug.WriteLine("Name conflict! " + tableName + "=" + className);
            }
            else
            {
                mappingNamer.EntityNames.Add(className);
            }
        }
    }
}
