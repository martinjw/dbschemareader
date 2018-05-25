using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    public class RepositoryImplementationWriter
    {
        private readonly DatabaseTable table;
        private readonly ClassBuilder classBuilder;
        private DataAnnotationWriter _dataAnnotationWriter;
        private readonly CodeWriterSettings codeWriterSettings;
        private IEnumerable<string> logicalDeleteColumns;

        public RepositoryImplementationWriter(DatabaseTable table, CodeWriterSettings codeWriterSettings,
            IEnumerable<string> logicalDeleteColumns)
        {
            this.codeWriterSettings = codeWriterSettings;
            this.table = table;
            classBuilder = new ClassBuilder();
            this.logicalDeleteColumns = logicalDeleteColumns;
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
            var comment = $"Repository class for the {table.Name} {tableOrView}";
            var classDefinition = $"public class {CodeWriterUtils.GetRepositoryImplementationName(table)} : {CodeWriterUtils.GetRepositoryInterfaceName(table)}";
            classBuilder.AppendXmlSummary(comment);
            classBuilder.BeginNest(classDefinition, comment);
            WriteAllMembers();
            classBuilder.EndNest(); // class
            classBuilder.EndNest(); // namespace
            return classBuilder.ToString();
        }

        private void WriteAllMembers()
        {
            classBuilder.AppendLine("private IDbContext dbContext;");
            classBuilder.AppendLine("");
            WriteConstructors();
            WriteCreate();
            WriteGets();
            WriteUpdate();
            WriteDelete();
        }

        private void WriteConstructors()
        {
            /*using (classBuilder.BeginNest($"public {CodeWriterUtils.GetRepositoryImplementationName(table)}()"))
            {
            }

            classBuilder.AppendLine("");*/
            using (classBuilder.BeginNest($"public {CodeWriterUtils.GetRepositoryImplementationName(table)}(IDbContext dbContext)"))
            {
                classBuilder.AppendLine("this.dbContext = dbContext;");
            }

            classBuilder.AppendLine("");
        }

        private void WriteUsings()
        {
            classBuilder.AppendLine("using System;");
            classBuilder.AppendLine("using System.Collections.Generic;");
            classBuilder.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            classBuilder.AppendLine("using System.Linq;");
            classBuilder.AppendLine("using System.Reflection;");
            classBuilder.AppendLine("using PeopleNet.EnterpriseData.DataAccess.Exceptions;");
            classBuilder.AppendLine("using PeopleNet.EnterpriseData.DataAccess.Repositories;");
            classBuilder.AppendLine("");
        }

        private void WriteGets()
        {
            // Get a list of primary keys
            // Find all combinations of the list
            // Write a GetSingle method for the combination of all primary keys
            // For each other combination, write a GetList method

            var primaryKeyColumns = table.Columns.Where(c => c.IsPrimaryKey);
            WriteGet();
            classBuilder.AppendLine("");

            WriteGetList();
            classBuilder.AppendLine("");

            var combinations = CodeWriterUtils.GetGetListByColumnCombinations(table)?.ToList();
            if (combinations == null)
            {
                return;
            }

            foreach (var c in combinations)
            {
                if (c.SequenceEqual(primaryKeyColumns))
                {
                    continue;
                }

                WriteGetListBy(c);
                classBuilder.AppendLine("");
            }
        }

        private void WriteGetListMethodSummary(IEnumerable<Parameter> methodParameters)
        {
            classBuilder.AppendXmlSummary(
                $"Queries the database for each instance.",
                $"A list of instances of <see cref=\"{table.NetName}\"/>, or an empty list if are none.",
                $"This method returns shallow instances of <see cref=\"{table.NetName}\"/>, i.e., it does not recurse.",
                null,
                methodParameters
            );
        }

        private void WriteGetList()
        {
            var methodParameters = CodeWriterUtils.GetGetListMethodParameters(table, codeWriterSettings);
            var methodParametersWithDbContext = new List<Parameter>(methodParameters);
            methodParametersWithDbContext.Insert(0, GetDbContextMethodParameter());

            WriteGetListMethodSummary(methodParametersWithDbContext);
            using (classBuilder.BeginNest($"public static {CodeWriterUtils.GetGetListMethodSignature(table, codeWriterSettings, methodParametersWithDbContext)}"))
            {
                var sqlCommandText = $@"$""SELECT {GetAllColumnNames()} FROM \""{table.Name}\"";""";
                var logicalDeleteColumn = table.Columns.SingleOrDefault(c => logicalDeleteColumns.Contains(c.Name));
                if (logicalDeleteColumn != null)
                {
                    sqlCommandText = $@"$""SELECT {GetAllColumnNames()} FROM \""{table.Name}\"" WHERE \""{ logicalDeleteColumn.Name}\"" IS NULL;""";
                }

                classBuilder.AppendLine($"var entities = new List<{table.NetName}>();");
                WriteExecuteReaderBlock(
                    sqlCommandText,
                    cb => { },
                    cb =>
                    {
                        using (cb.BeginNest("while (reader.Read())"))
                        {
                            classBuilder.AppendLine($"var entity = new {table.NetName}();");
                            WriteParseEntityFromReader("entity");
                            classBuilder.AppendLine("entity.DbContext = dbContext;");
                            classBuilder.AppendLine("entities.Add(entity);");
                        }
                    });
                classBuilder.AppendLine("");
                classBuilder.AppendLine("return entities;");
            }

            classBuilder.AppendLine("");
            WriteGetListMethodSummary(methodParameters);
            using (classBuilder.BeginNest($"public {CodeWriterUtils.GetGetListMethodSignature(table, codeWriterSettings, methodParameters)}"))
            {
                classBuilder.AppendLine($"return GetList({PrintParametersForCall(methodParametersWithDbContext)});");
            }

            classBuilder.AppendLine("");
        }

        private void WriteGetListByMethodSummary(IEnumerable<Parameter> methodParameters)
        {
            classBuilder.AppendXmlSummary(
                $"Queries the database for each instance whose properties match the specified values.",
                $"A list of instances of <see cref=\"{table.NetName}\"/>, or an empty list if there are no matches.",
                $"This method returns shallow instances of <see cref=\"{table.NetName}\"/>, i.e., it does not recurse.",
                null,
                methodParameters
            );
        }

        private void WriteGetListBy(IEnumerable<DatabaseColumn> columns)
        {
            var methodParameters = CodeWriterUtils.GetMethodParametersForColumns(columns, codeWriterSettings);
            var methodParametersWithDbContext = new List<Parameter>(methodParameters);
            methodParametersWithDbContext.Insert(0, GetDbContextMethodParameter());

            WriteGetListByMethodSummary(methodParametersWithDbContext);
            using (classBuilder.BeginNest(
                $"public static {CodeWriterUtils.GetGetListByMethodSignature(table, columns, codeWriterSettings, methodParametersWithDbContext)}")
            )
            {
                var whereClause = GetWhereClauseFromMethodParameters(methodParameters);
                var sqlCommandText =
                    $"$\"SELECT {GetAllColumnNames()} FROM \\\"{table.Name}\\\" WHERE {whereClause};\"";
                classBuilder.AppendLine($"var entities = new List<{table.NetName}>();");
                WriteExecuteReaderBlock(
                    sqlCommandText, cb =>
                    {
                        foreach (var mp in methodParameters)
                        {
                            classBuilder.AppendLine($"dbContext.AddParameter(command, \"@{mp.Name}\", {mp.Name});");
                        }
                    },
                    cb =>
                    {
                        using (cb.BeginNest("while (reader.Read())"))
                        {
                            classBuilder.AppendLine($"var entity = new {table.NetName}();");
                            WriteParseEntityFromReader("entity");
                            classBuilder.AppendLine("entity.DbContext = dbContext;");
                            classBuilder.AppendLine("entities.Add(entity);");
                        }
                    });
                classBuilder.AppendLine("");
                classBuilder.AppendLine("return entities;");
            }

            classBuilder.AppendLine("");
            WriteGetListByMethodSummary(methodParameters);
            using (classBuilder.BeginNest($"public {CodeWriterUtils.GetGetListByMethodSignature(table, columns, codeWriterSettings, methodParameters)}"))
            {
                classBuilder.AppendLine($"return {CodeWriterUtils.GetGetListByMethodName(columns, codeWriterSettings)}({PrintParametersForCall(methodParametersWithDbContext)});");
            }
        }

        private string GetWhereClauseFromMethodParameters(List<Parameter> methodParameters)
        {
            var whereClauseElements = methodParameters.Select(mp => $"\\\"{mp.ColumnNameToQueryBy}\\\" = @{mp.Name}").ToList();
            var logicalDeleteColumn = table.Columns.SingleOrDefault(c => logicalDeleteColumns.Contains(c.Name));
            if (logicalDeleteColumn != null)
            {
                whereClauseElements.Add($"\\\"{logicalDeleteColumn.Name}\\\" IS NULL");
            }

            var whereClause = string.Join(" AND ", whereClauseElements);
            return whereClause;
        }

        private void WriteDeleteMethodSummary(IEnumerable<Parameter> methodParameters)
        {
            classBuilder.AppendXmlSummary(
                $"Deletes the specified <see cref=\"{table.NetName}\"/> from the database.",
                $"The deleted instance of <see cref=\"{table.NetName}\"/> with fully-populated and updated properties (logical/soft delete), or <c>null</c> (physical/hard delete).",
                $"Logical/soft delete is performed if possible (i.e., the table has a column for storing the deleted timestamp).",
                new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("EntityNotFoundException", "<paramref name=\"entity\"/> is not found in the database."),
                    new Tuple<string, string>("EntityHasDependenciesException", "<paramref name=\"entity\"/> cannot be deleted because it is still referenced in the database.")
                },
                methodParameters);
        }

        private void WriteDelete()
        {
            var isLogicalDelete = table.Columns.Any(c => logicalDeleteColumns.Contains(c.Name));
            if (isLogicalDelete)
            {
                WriteDeleteLogical();
                classBuilder.AppendLine("");
            }

            WriteDeletePhysical();
            classBuilder.AppendLine("");

            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings).ToList();
            var methodParametersWithDbContext = new List<Parameter>(methodParameters);
            methodParametersWithDbContext.Insert(0, GetDbContextMethodParameter());

            WriteDeleteMethodSummary(methodParametersWithDbContext);
            using (classBuilder.BeginNest(
                $"public static {CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, methodParametersWithDbContext)}")
            )
            {
                if (isLogicalDelete)
                {
                    classBuilder.AppendLine(
                        $"var deletedEntity = DeleteLogical({PrintParametersForCall(methodParametersWithDbContext)});");
                    classBuilder.BeginNest($"if (deletedEntity == null)");
                    classBuilder.AppendLine($"throw new EntityNotFoundException();");
                    classBuilder.EndNest();
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("return deletedEntity;");
                }
                else
                {
                    classBuilder.AppendLine("int countRowsAffected = -1;");
                    classBuilder.BeginNest("try");
                    classBuilder.AppendLine(
                        $"countRowsAffected = DeletePhysical({PrintParametersForCall(methodParametersWithDbContext)});");
                    classBuilder.EndNest();
                    classBuilder.BeginNest("catch (Exception e)");
                    classBuilder.BeginNest($"if (e is Npgsql.PostgresException pge && pge.SqlState == \"23503\")");
                    classBuilder.AppendLine("throw new EntityHasDependenciesException();");
                    classBuilder.EndNest();
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("throw;");
                    classBuilder.EndNest();
                    classBuilder.AppendLine("");
                    classBuilder.BeginNest("if (countRowsAffected == 0)");
                    classBuilder.AppendLine("throw new EntityNotFoundException();");
                    classBuilder.EndNest();
                    classBuilder.AppendLine("");
                    classBuilder.BeginNest("if (countRowsAffected != 1)");
                    classBuilder.AppendLine("throw new Exception(\"Delete affected more than one row.\");");
                    classBuilder.EndNest();
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("return null;");
                }
            }

            classBuilder.AppendLine("");
            WriteDeleteMethodSummary(methodParameters);
            using (classBuilder.BeginNest($"public {CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, methodParameters)}"))
            {
                classBuilder.AppendLine($"return Delete({PrintParametersForCall(methodParametersWithDbContext)});");
            }

            classBuilder.AppendLine("");
        }

        private void WriteDeleteLogical()
        {
            var methodParameters = CodeWriterUtils.GetMethodParametersForColumns(table.Columns.Where(c => c.IsPrimaryKey), codeWriterSettings);
            methodParameters.Insert(0, GetDbContextMethodParameter());
            using (classBuilder.BeginNest($"private static {table.NetName} DeleteLogical({CodeWriterUtils.PrintParametersForSignature(methodParameters)})"))
            {
                var whereClause = GetWhereClauseFromMethodParameters(methodParameters);
                var logicalDeleteColumn = table.Columns.Single(c => logicalDeleteColumns.Contains(c.Name));
                var setClause = $"\\\"{logicalDeleteColumn.Name}\\\" = NOW()";
                var sqlCommandText = $"\"UPDATE \\\"{table.Name}\\\" SET {setClause} WHERE {whereClause} RETURNING {GetAllColumnNames()};\";";
                var entityVariableName = "deletedEntity";
                classBuilder.AppendLine($"{table.NetName} {entityVariableName} = null;");
                WriteExecuteReaderBlock(
                    sqlCommandText,
                    cb =>
                    {
                        foreach (var mp in methodParameters)
                        {
                            classBuilder.AppendLine($"dbContext.AddParameter(command, \"@{mp.Name}\", {mp.Name});");
                        }
                    },
                    cb =>
                    {
                        using (cb.BeginNest("if (reader.Read())"))
                        {
                            classBuilder.AppendLine($"{entityVariableName} = new {table.NetName}();");
                            WriteParseEntityFromReader(entityVariableName);
                        }
                    });
                WriteReturnEntityIfNotNull(entityVariableName);
            }
        }

        private Parameter GetDbContextMethodParameter()
        {
            return new Parameter()
            {
                DataType = "IDbContext",
                Name = "dbContext",
                Summary = "A database context."
            };
        }

        private void WriteDeletePhysical()
        {
            var methodParameters = CodeWriterUtils.GetMethodParametersForColumns(table.Columns.Where(c => c.IsPrimaryKey), codeWriterSettings);
            methodParameters.Insert(0, GetDbContextMethodParameter());
            classBuilder.BeginNest($"private static int DeletePhysical({CodeWriterUtils.PrintParametersForSignature(methodParameters)})");
            var wc = string.Join(" AND ", methodParameters.Select(mp => $"\\\"{mp.ColumnNameToQueryBy}\\\" = @{mp.Name}"));

            var sql = $"DELETE FROM \\\"{table.Name}\\\" WHERE {wc};";
            classBuilder.BeginNest($"using (var connection = dbContext.CreateConnection())");
            classBuilder.BeginNest($"using (var command = connection.CreateCommand())");
            classBuilder.AppendLine($"command.CommandText = \"{sql}\";");
            foreach (var mp in methodParameters)
            {
                classBuilder.AppendLine($"dbContext.AddParameter(command, \"{mp.Name}\", {mp.Name});");
            }

            classBuilder.AppendLine("connection.Open();");
            classBuilder.AppendLine($"return command.ExecuteNonQuery();");
            classBuilder.EndNest();
            classBuilder.EndNest();
            classBuilder.EndNest();
        }

        private void WriteUpdateMethodSummary(IEnumerable<Parameter> methodParameters)
        {
            classBuilder.AppendXmlSummary(
                $"Updates the specified <see cref=\"{table.NetName}\"/> in the database.",
                $"The updated instance of <see cref=\"{table.NetName}\"/> with fully-populated and updated properties.",
                $"Method parameters specify the entity to be updated. Properties on <paramref name=\"entity\"/> that correspond to primary key columns on the \"{table.Name}\" table are not updated.",
                null,
                methodParameters
            );
        }

        private void WriteUpdate()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings);
            var methodParametersWithDbContext = new List<Parameter>(methodParameters);
            methodParametersWithDbContext.Insert(0, GetDbContextMethodParameter());

            WriteUpdateMethodSummary(methodParametersWithDbContext);
            using (classBuilder.BeginNest(
                $"public static {CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, methodParametersWithDbContext)}")
            )
            {
                WriteGetPropertyColumnPairs();
                var whereClause = GetWhereClauseFromMethodParameters(methodParameters.ToList());
                classBuilder.AppendLine(
                    "var setClause = string.Join(\", \", propertyColumnPairs.Select(pcp => $\"{pcp.Value} = @{pcp.Key.Name}\"));");
                classBuilder.AppendLine(
                    $"var sqlCommandText = $\"UPDATE \\\"{table.Name}\\\" SET {{setClause}} WHERE {whereClause} RETURNING {GetAllColumnNames()};\";");
                var entityVariableName = "updatedEntity";
                classBuilder.AppendLine($"{table.NetName} {entityVariableName} = null;");
                WriteExecuteReaderBlock(
                    "sqlCommandText",
                    cb =>
                    {
                        foreach (var mp in methodParameters)
                        {
                            classBuilder.AppendLine($"dbContext.AddParameter(command, \"@{mp.Name}\", {mp.Name});");
                        }

                        using (classBuilder.BeginNest($"foreach (var key in propertyColumnPairs.Keys)"))
                        {
                            classBuilder.AppendLine(
                                "dbContext.AddParameter(command, $\"@{key.Name}\", key.GetValue(entity));");
                        }
                    },
                    cb =>
                    {
                        using (cb.BeginNest("if (reader.Read())"))
                        {
                            classBuilder.AppendLine($"{entityVariableName} = new {table.NetName}();");
                            WriteParseEntityFromReader(entityVariableName);
                        }
                    });
                WriteReturnEntityIfNotNull(entityVariableName);
            }

            classBuilder.AppendLine("");
            WriteUpdateMethodSummary(methodParameters);
            using (classBuilder.BeginNest($"public {CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, methodParameters)}"))
            {
                classBuilder.AppendLine($"return Update({PrintParametersForCall(methodParametersWithDbContext)});");
            }

            classBuilder.AppendLine("");
        }

        private void WriteCreateMethodSummary(IEnumerable<Parameter> methodParameters)
        {
            classBuilder.AppendXmlSummary(
                $"Inserts the specified <see cref=\"{table.NetName}\"/> to the database.",
                $"The inserted instance of <see cref=\"{table.NetName}\"/> with fully-populated properties.",
                $"This method ignores properties on <see cref=\"{table.NetName}\"/> that correspond to columns with auto-generated sequences, and properties whose values are default and corresond to nullable columns with default values.",
                null,
                methodParameters
            );
        }

        private void WriteCreate()
        {
            var methodParameters = CodeWriterUtils.GetCreateMethodParameters(table).ToList();
            var methodParametersWithDbContext = new List<Parameter>(methodParameters);
            methodParametersWithDbContext.Insert(0, GetDbContextMethodParameter());

            WriteCreateMethodSummary(methodParametersWithDbContext);
            using (classBuilder.BeginNest($"public static {CodeWriterUtils.GetCreateMethodSignature(table, methodParametersWithDbContext)}"))
            {
                WriteGetPropertyColumnPairs();
                classBuilder.AppendLine("var valuesClause = string.Join(\", \", propertyColumnPairs.Keys.Select(k => \"@\" + k.Name));");
                classBuilder.AppendLine($"var sqlCommandText = $\"INSERT INTO \\\"{table.Name}\\\" ({{string.Join(\", \", propertyColumnPairs.Values)}}) VALUES ({{valuesClause}}) RETURNING {GetAllColumnNames()};\";");

                var entityVariableName = "createdEntity";
                classBuilder.AppendLine($"{table.NetName} {entityVariableName} = null;");
                WriteExecuteReaderBlock(
                    "sqlCommandText", cb =>
                    {
                        using (classBuilder.BeginNest($"foreach (var key in propertyColumnPairs.Keys)"))
                        {
                            classBuilder.AppendLine("dbContext.AddParameter(command, $\"@{key.Name}\", key.GetValue(entity));");
                        }
                    },
                    cb =>
                    {
                        using (cb.BeginNest("if (reader.Read())"))
                        {
                            classBuilder.AppendLine($"{entityVariableName} = new {table.NetName}();");
                            WriteParseEntityFromReader(entityVariableName);
                        }
                    });
                WriteReturnEntityIfNotNull(entityVariableName);
            }

            classBuilder.AppendLine("");
            WriteCreateMethodSummary(methodParameters);
            using (classBuilder.BeginNest($"public {CodeWriterUtils.GetCreateMethodSignature(table, methodParameters)}"))
            {
                classBuilder.AppendLine($"return Create({PrintParametersForCall(methodParametersWithDbContext)});");
            }

            classBuilder.AppendLine("");
        }

        private string PrintParametersForCall(IEnumerable<Parameter> methodParameters)
        {
            return string.Join(", ", methodParameters.Select(mp => $"{mp.Name}"));
        }


        private string GetAllColumnNames()
        {
            return string.Join(", ", table.Columns.Select(c => $"\\\"{c.Name}\\\""));
        }

        private void WriteGetPropertyColumnPairs()
        {
            classBuilder.AppendLine($"var columnProperties = entity.GetType().GetProperties().Where(p => p.IsDefined(typeof(ColumnAttribute), false));");
            classBuilder.AppendLine($"var propertyColumnPairs = new Dictionary<PropertyInfo, string>();");
            using (classBuilder.BeginNest($"foreach (var cp in columnProperties)"))
            {
                classBuilder.AppendLine($"var columnAttribute = (ColumnAttribute)cp.GetCustomAttribute(typeof(ColumnAttribute));");
                classBuilder.AppendLine($"var dbGeneratedAttribute = (DatabaseGeneratedAttribute)cp.GetCustomAttribute(typeof(DatabaseGeneratedAttribute));");
                using (classBuilder.BeginNest($"if (dbGeneratedAttribute != null && dbGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)")
                )
                {
                    classBuilder.AppendLine("continue;");
                }

                classBuilder.AppendLine("");
                using (classBuilder.BeginNest($"if (dbGeneratedAttribute != null && dbGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Computed)"))
                {
                    classBuilder.AppendLine($"var defaultValue = cp.PropertyType.IsValueType ? Activator.CreateInstance(cp.PropertyType) : null;");
                    using (classBuilder.BeginNest($"if (cp.GetValue(entity).Equals(defaultValue))"))
                    {
                        classBuilder.AppendLine("continue;");
                    }
                }

                classBuilder.AppendLine("");
                classBuilder.AppendLine($"propertyColumnPairs.Add(cp, columnAttribute.Name);");
            }

            classBuilder.AppendLine("");
        }

        private void WriteGetMethodSummary(IEnumerable<Parameter> methodParameters)
        {
            classBuilder.AppendXmlSummary(
                $"Queries the database for a single instance whose properties match the specified values.",
                $"An instance of <see cref=\"{table.NetName}\"/>, or <c>null</c> if there is no match.",
                $"This method gets only primitive properties, i.e., only properties that correspond to columns on the database table. No recursion is performed.",
                new List<Tuple<string, string>>()
                {
                    new Tuple<string, string>("ArgumentNullException", "<paramref name=\"filter\"/> is <c>null</c> or empty."),
                    new Tuple<string, string>("InvalidOperationException", "There are multiple matches in the database.")
                },
                methodParameters);
        }

        private void WriteGet()
        {
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings).ToList();
            var methodParametersWithDbContext = new List<Parameter>(methodParameters);
            methodParametersWithDbContext.Insert(0, GetDbContextMethodParameter());

            WriteGetMethodSummary(methodParametersWithDbContext);
            using (classBuilder.BeginNest(
                $"public static {CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParametersWithDbContext)}")
            )
            {
                var whereClause = GetWhereClauseFromMethodParameters(methodParameters);
                var sqlCommandText =
                    $"$\"SELECT {GetAllColumnNames()} FROM \\\"{table.Name}\\\" WHERE {whereClause};\"";
                var entityVariableName = "entity";
                classBuilder.AppendLine($"{table.NetName} {entityVariableName} = null;");
                WriteExecuteReaderBlock(
                    sqlCommandText, cb =>
                    {
                        foreach (var mp in methodParameters)
                        {
                            classBuilder.AppendLine($"dbContext.AddParameter(command, \"@{mp.Name}\", {mp.Name});");
                        }
                    },
                    cb =>
                    {
                        using (cb.BeginNest("if (reader.Read())"))
                        {
                            classBuilder.AppendLine($"{entityVariableName} = new {table.NetName}();");
                            WriteParseEntityFromReader(entityVariableName);
                        }
                    });
                WriteReturnEntityIfNotNull(entityVariableName);
            }

            classBuilder.AppendLine("");
            WriteGetMethodSummary(methodParameters);
            using (classBuilder.BeginNest(
                $"public {CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParameters)}"))
            {
                classBuilder.AppendLine($"return Get({PrintParametersForCall(methodParametersWithDbContext)});");
            }

            classBuilder.AppendLine("");
        }

        private void WriteExecuteReaderBlock(string sqlCommandText, Action<ClassBuilder> addCommandParameters, Action<ClassBuilder> processReader)
        {
            using (classBuilder.BeginNest($"using (var connection = dbContext.CreateConnection())"))
            {
                classBuilder.AppendLine("connection.Open();");
                using (classBuilder.BeginNest($"using (var command = connection.CreateCommand())"))
                {
                    classBuilder.AppendLine($"command.CommandText = {sqlCommandText};");
                    addCommandParameters(classBuilder);

                    using (classBuilder.BeginNest("using (var reader = command.ExecuteReader())"))
                    {
                        processReader(classBuilder);
                    }
                }
            }
        }

        private void WriteReturnEntityIfNotNull(string entityVariableName)
        {
            classBuilder.AppendLine("");
            using (classBuilder.BeginNest($"if ({entityVariableName} == null)"))
            {
                classBuilder.AppendLine("return null;");
            }

            classBuilder.AppendLine("");
            classBuilder.AppendLine($"{entityVariableName}.DbContext = dbContext;");
            classBuilder.AppendLine($"return {entityVariableName};");
        }

        private void WriteParseEntityFromReader(string entityVariableName)
        {
            foreach (var c in table.Columns.Where(c => c.DbDataType != "geometry" && c.DbDataType != "geography"))
            {
                if (c.Nullable)
                {
                    classBuilder.AppendLine($"{entityVariableName}.{CodeWriterUtils.GetPropertyNameForDatabaseColumn(c)} = reader.GetValue({c.Ordinal - 1}) == DBNull.Value ? null : ({DataTypeWriter.FindDataType(c)})reader.GetValue({c.Ordinal - 1});");
                }
                else
                {
                    classBuilder.AppendLine($"{entityVariableName}.{CodeWriterUtils.GetPropertyNameForDatabaseColumn(c)} = ({DataTypeWriter.FindDataType(c)})reader.GetValue({c.Ordinal - 1});");
                }
            }
        }
    }
}
