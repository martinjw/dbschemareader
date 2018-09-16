using DatabaseSchemaReader.DataSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DatabaseSchemaReader.CodeGen
{
    public class RepositoryImplementationWriter
    {
        private DatabaseTable table;
        private ClassBuilder classBuilder;
        private IEnumerable<string> logicalDeleteColumns;
        private CodeWriterSettings codeWriterSettings { get; }
        private DatabaseSchema schema { get; }
        private string _serviceProviderFieldName = "_serviceProvider";
        private string _loggerFieldName = "_logger";
        private string _dbContextFieldName = "_dbContext";

        public RepositoryImplementationWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings, IEnumerable<string> logicalDeleteColumns)
        {
            this.codeWriterSettings = codeWriterSettings;
            this.schema = schema;
            this.logicalDeleteColumns = logicalDeleteColumns;
        }

        public void Execute()
        {
            foreach (var t in schema.Tables)
            {
                table = t;
                classBuilder = new ClassBuilder();
                var implementationText = Write();
                CodeWriterUtils.WriteClassFile(codeWriterSettings.OutputDirectory, CodeWriterUtils.GetRepositoryImplementationName(table), implementationText);
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
            var comment = $"Repository class for the {table.Name} {tableOrView}";
            var classDefinition = $"public partial class {CodeWriterUtils.GetRepositoryImplementationName(table)} : {CodeWriterUtils.GetRepositoryInterfaceName(table)}";
            classBuilder.AppendXmlSummary(comment);
            classBuilder.BeginNest(classDefinition);
            WriteAllMembers();
            classBuilder.EndNest(); // class
            classBuilder.EndNest(); // namespace
            return classBuilder.ToString();
        }

        private void WriteAllMembers()
        {
            WriteConstructorsAndFields();
            WriteCreate();
            WriteGets();
            WriteGetLists();
            WriteGetListBys();
            WriteUpdates();
            WriteDeletes();
        }

        private void WriteDeletes()
        {
            var isLogicalDelete = table.Columns.Any(c => logicalDeleteColumns.Contains(c.Name));
            WriteDelete(isLogicalDelete);
            WriteDeleteByCustomer(isLogicalDelete);
            WriteDeleteUnique(isLogicalDelete);
            WriteDeleteUniqueByCustomer(isLogicalDelete);
        }

        private void WriteDelete(bool isLogicalDelete)
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, false, false);
            if (isLogicalDelete)
            {
                WriteDeleteLogical(methodParameters);
                classBuilder.AppendLine("");
            }

            WriteDeletePhysical(methodParameters);
            classBuilder.AppendLine("");
            WriteDeleteCommon(methodParameters, isLogicalDelete);
        }

        private void WriteDeleteByCustomer(bool isLogicalDelete)
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, true, false);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            if (isLogicalDelete)
            {
                WriteDeleteLogicalByCustomer(methodParameters);
                classBuilder.AppendLine("");
            }

            WriteDeletePhysicalByCustomer(methodParameters);
            classBuilder.AppendLine("");
            WriteDeleteCommon(methodParameters, isLogicalDelete);
        }

        private void WriteDeleteUnique(bool isLogicalDelete)
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, false, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            if (isLogicalDelete)
            {
                WriteDeleteLogicalUnique(methodParameters);
                classBuilder.AppendLine("");
            }

            WriteDeletePhysicalUnique(methodParameters);
            classBuilder.AppendLine("");
            WriteDeleteCommon(methodParameters, isLogicalDelete);
        }

        private void WriteDeleteUniqueByCustomer(bool isLogicalDelete)
        {
            var methodParameters = CodeWriterUtils.GetDeleteMethodParameters(table, codeWriterSettings, true, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            if (isLogicalDelete)
            {
                WriteDeleteLogicalUniqueByCustomer(methodParameters);
                classBuilder.AppendLine("");
            }

            WriteDeletePhysicalUniqueByCustomer(methodParameters);
            classBuilder.AppendLine("");
            WriteDeleteCommon(methodParameters, isLogicalDelete);
        }

        private void WriteDeleteCommon(IEnumerable<Parameter> methodParameters, bool isLogicalDelete)
        {
            var partialMethodName = CodeWriterUtils.ConvertParametersToMethodNameByPart(methodParameters, codeWriterSettings);
            WriteDeleteMethodSummary(methodParameters);
            var methodSignature = CodeWriterUtils.GetDeleteMethodSignature(table, codeWriterSettings, methodParameters);
            using (classBuilder.BeginNest($"public {methodSignature}"))
            {
                CodeWriterUtils.WriteEntryLogging(classBuilder, methodSignature);
                if (isLogicalDelete)
                {
                    classBuilder.AppendLine($"var deletedEntity = {CodeWriterUtils.BaseMethodNameDelete}LogicalBy{partialMethodName}({PrintParametersForCall(methodParameters)});");
                    classBuilder.BeginNest($"if (deletedEntity == null)");
                    classBuilder.AppendLine($"throw new EntityNotFoundException();");
                    classBuilder.EndNest();
                    classBuilder.AppendLine("");
                    classBuilder.AppendLine("return deletedEntity;");
                }
                else
                {
                    classBuilder.AppendLine($"int countRowsAffected = {CodeWriterUtils.BaseMethodNameDelete}PhysicalBy{partialMethodName}({PrintParametersForCall(methodParameters)});");
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

                CodeWriterUtils.WriteExitLogging(classBuilder, methodSignature);
            }
        }

        private void WriteDeleteLogical(IEnumerable<Parameter> methodParameters)
        {
            var whereClause = GetWhereClauseFromMethodParameters(methodParameters);
            WriteDeleteLogicalCommon(methodParameters, null, whereClause, GetAllColumnNames(new List<DatabaseTable> { table }));
        }

        private void WriteDeleteLogicalByCustomer(IEnumerable<Parameter> methodParameters)
        {
            var orgUnitTableAlias = codeWriterSettings.Namer.NameToAcronym(CodeWriterUtils.CustomerAssetOrganizationTableName);
            var fromClause = $"\\\"{CodeWriterUtils.CustomerAssetOrganizationTableName}\\\" {orgUnitTableAlias}";
            var whereClause = GetWhereClauseFromMethodParameters(methodParameters);
            var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
            var joinOnClause = $"{thisTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\" = {orgUnitTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\"";
            whereClause = $"{joinOnClause} AND {whereClause}";
            WriteDeleteLogicalCommon(methodParameters, fromClause, whereClause, GetAllColumnNamesByCustomer());
        }

        private void WriteDeleteLogicalUnique(IEnumerable<Parameter> methodParameters)
        {
            WriteDeleteLogical(methodParameters);
        }

        private void WriteDeleteLogicalUniqueByCustomer(IEnumerable<Parameter> methodParameters)
        {
            WriteDeleteLogicalByCustomer(methodParameters);
        }

        private void WriteAddParametersToCommand(IEnumerable<Parameter> parameters)
        {
            foreach (var mp in parameters)
            {
                classBuilder.AppendLine($"{_dbContextFieldName}.AddParameter(command, \"@{mp.Name}\", {mp.Name});");
            }
        }

        private void WriteBeginReadReaderSingle()
        {
            classBuilder.BeginNest("if (reader.Read())");
        }

        private void WriteDeleteLogicalCommon(IEnumerable<Parameter> methodParameters, string fromClause, string whereClause, string columnsToReturn)
        {
            var partialMethodName = CodeWriterUtils.ConvertParametersToMethodNameByPart(methodParameters, codeWriterSettings);
            using (classBuilder.BeginNest($"private {table.NetName} {CodeWriterUtils.BaseMethodNameDelete}LogicalBy{partialMethodName}({CodeWriterUtils.PrintParametersForSignature(methodParameters)})"))
            {
                var logicalDeleteColumn = table.Columns.Single(c => logicalDeleteColumns.Contains(c.Name));
                var setClause = $"\\\"{logicalDeleteColumn.Name}\\\" = NOW()";
                var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
                var sqlCommandText = $"\"UPDATE ONLY \\\"{table.Name}\\\" AS {thisTableAlias} SET {setClause}";
                if (!string.IsNullOrEmpty(fromClause))
                {
                    sqlCommandText = $"{sqlCommandText} FROM {fromClause}";
                }

                sqlCommandText = $"{sqlCommandText} WHERE {whereClause} RETURNING {columnsToReturn};\";";
                var entityVariableName = "deletedEntity";
                classBuilder.AppendLine($"{table.NetName} {entityVariableName} = null;");
                WriteBeginCreateConnection();
                WriteBeginCreateCommand();
                classBuilder.AppendLine($"command.CommandText = {sqlCommandText};");
                WriteAddParametersToCommand(methodParameters);
                WriteBeginExecuteReader();
                WriteBeginReadReaderSingle();
                // TODO: KE - consider throwing here if multiple rows were modified! It should never be the case except for bad data even though the schema allows it
                classBuilder.AppendLine($"{entityVariableName} = ({table.NetName}){_serviceProviderFieldName}.GetService(typeof({table.NetName}));");
                WriteParseEntityFromReader(entityVariableName);
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                WriteReturnEntityIfNotNull(entityVariableName);
            }
        }

        private void WriteDeletePhysical(IEnumerable<Parameter> methodParameters)
        {
            var whereClause = GetWhereClauseFromMethodParameters(methodParameters);
            WriteDeletePhysicalCommon(methodParameters, null, whereClause);
        }

        private void WriteDeletePhysicalByCustomer(IEnumerable<Parameter> methodParameters)
        {
            var orgUnitTableAlias = codeWriterSettings.Namer.NameToAcronym(CodeWriterUtils.CustomerAssetOrganizationTableName);
            var usingClause = $"\\\"{CodeWriterUtils.CustomerAssetOrganizationTableName}\\\" {orgUnitTableAlias}";
            var whereClause = GetWhereClauseFromMethodParameters(methodParameters);
            var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
            var joinOnClause = $"{thisTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\" = {orgUnitTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\"";
            whereClause = $"{joinOnClause} AND {whereClause}";
            WriteDeletePhysicalCommon(methodParameters, usingClause, whereClause);
        }

        private void WriteDeletePhysicalUnique(IEnumerable<Parameter> methodParameters)
        {
            WriteDeletePhysical(methodParameters);
        }

        private void WriteDeletePhysicalUniqueByCustomer(IEnumerable<Parameter> methodParameters)
        {
            WriteDeletePhysicalByCustomer(methodParameters);
        }

        private void WriteDeletePhysicalCommon(IEnumerable<Parameter> methodParameters, string usingClause, string whereClause)
        {
            var partialMethodName = CodeWriterUtils.ConvertParametersToMethodNameByPart(methodParameters, codeWriterSettings);
            classBuilder.BeginNest($"private int {CodeWriterUtils.BaseMethodNameDelete}PhysicalBy{partialMethodName}({CodeWriterUtils.PrintParametersForSignature(methodParameters)})");
            var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
            var sqlCommandText = $"\"DELETE FROM ONLY \\\"{table.Name}\\\" AS {thisTableAlias}";
            if (!string.IsNullOrEmpty(usingClause))
            {
                sqlCommandText = $"{sqlCommandText} USING {usingClause}";
            }

            sqlCommandText = $"{sqlCommandText} WHERE {whereClause};\"";
            WriteBeginCreateConnection();
            WriteBeginCreateCommand();
            classBuilder.AppendLine($"command.CommandText = {sqlCommandText};");
            WriteAddParametersToCommand(methodParameters);
            classBuilder.BeginNest("try");
            classBuilder.AppendLine($"return command.ExecuteNonQuery();");
            classBuilder.EndNest();
            classBuilder.BeginNest("catch (Exception e)");
            classBuilder.BeginNest($"if (e is Npgsql.PostgresException pge && pge.SqlState == \"23503\")");
            classBuilder.AppendLine("throw new EntityHasDependenciesException();");
            classBuilder.EndNest();
            classBuilder.AppendLine("");
            classBuilder.AppendLine("throw;");
            classBuilder.EndNest();
            classBuilder.EndNest();
            classBuilder.EndNest();
            // TODO: KE - consider throwing here if multiple rows were modified! It should never be the case except for bad data even though the schema allows it
            classBuilder.EndNest();
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
            var whereClause = GetWhereClauseFromMethodParameters(methodParameters.ToList());
            WriteUpdateCommon(methodParameters, null, whereClause, GetAllColumnNames(new List<DatabaseTable> { table }));
        }

        private void WriteUpdateByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings, true, false);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            var orgUnitTableAlias = codeWriterSettings.Namer.NameToAcronym(CodeWriterUtils.CustomerAssetOrganizationTableName);
            var fromClause = $"\\\"{CodeWriterUtils.CustomerAssetOrganizationTableName}\\\" {orgUnitTableAlias}";
            var whereClause = GetWhereClauseFromMethodParameters(methodParameters.ToList());
            var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
            var joinOnClause = $"{thisTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\" = {orgUnitTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\"";
            whereClause = $"{joinOnClause} AND {whereClause}";
            WriteUpdateCommon(methodParameters, fromClause, whereClause, GetAllColumnNamesByCustomer());
        }

        private void WriteUpdateUnique()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings, false, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            var whereClause = GetWhereClauseFromMethodParameters(methodParameters.ToList());
            WriteUpdateCommon(methodParameters, null, whereClause, GetAllColumnNames(new List<DatabaseTable> { table }));
        }

        private void WriteUpdateUniqueByCustomer()
        {
            var methodParameters = CodeWriterUtils.GetUpdateMethodParameters(table, codeWriterSettings, true, true);
            if (methodParameters == null || !methodParameters.Any())
            {
                return;
            }

            var orgUnitTableAlias = codeWriterSettings.Namer.NameToAcronym(CodeWriterUtils.CustomerAssetOrganizationTableName);
            var fromClause = $"\\\"{CodeWriterUtils.CustomerAssetOrganizationTableName}\\\" {orgUnitTableAlias}";
            var whereClause = GetWhereClauseFromMethodParameters(methodParameters.ToList());
            var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
            var joinOnClause = $"{thisTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\" = {orgUnitTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\"";
            whereClause = $"{joinOnClause} AND {whereClause}";
            WriteUpdateCommon(methodParameters, fromClause, whereClause, GetAllColumnNamesByCustomer());
        }

        private void WriteAddParametersToCommand()
        {
            using (classBuilder.BeginNest($"foreach (var key in propertyColumnPairs.Keys)"))
            {
                classBuilder.AppendLine($"{_dbContextFieldName}.AddParameter(command, $\"@{{key.Name}}\", key.GetValue(entity));");
            }
        }

        private void WriteUpdateCommon(IEnumerable<Parameter> methodParameters, string fromClause, string whereClause, string columnsToReturn)
        {
            var entityParameterSummary = "An entity with updated values.";
            var methodParametersWithEntity = CodeWriterUtils.AddEntityParameter(methodParameters, table, entityParameterSummary);
            WriteUpdateMethodSummary(methodParametersWithEntity);
            var methodSignature = CodeWriterUtils.GetUpdateMethodSignature(table, codeWriterSettings, methodParametersWithEntity);
            using (classBuilder.BeginNest($"public {methodSignature}"))
            {
                CodeWriterUtils.WriteEntryLogging(classBuilder, methodSignature);
                WriteGetPropertyColumnPairs();

                classBuilder.AppendLine("var setClause = string.Join(\", \", propertyColumnPairs.Select(pcp => $\"{pcp.Value} = @{pcp.Key.Name}\"));");
                var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
                if (!string.IsNullOrEmpty(fromClause))
                {
                    classBuilder.AppendLine($"var sqlCommandText = $\"UPDATE ONLY \\\"{table.Name}\\\" AS {thisTableAlias} SET {{setClause}} FROM {fromClause} WHERE {whereClause} RETURNING {columnsToReturn};\";");
                }
                else
                {
                    classBuilder.AppendLine($"var sqlCommandText = $\"UPDATE ONLY \\\"{table.Name}\\\" AS {thisTableAlias} SET {{setClause}} WHERE {whereClause} RETURNING {columnsToReturn};\";");
                }

                var entityVariableName = "updatedEntity";
                classBuilder.AppendLine($"{table.NetName} {entityVariableName} = null;");
                WriteBeginCreateConnection();
                WriteBeginCreateCommand();
                classBuilder.AppendLine($"command.CommandText = sqlCommandText;");
                WriteAddParametersToCommand(methodParameters);
                WriteAddParametersToCommand();
                classBuilder.BeginNest("try");
                WriteBeginExecuteReader();
                WriteBeginReadReaderSingle();
                // TODO: KE - consider throwing here if multiple rows were modified! It should never be the case except for bad data even though the schema allows it
                classBuilder.AppendLine($"{entityVariableName} = ({table.NetName}){_serviceProviderFieldName}.GetService(typeof({table.NetName}));");
                WriteParseEntityFromReader(entityVariableName);
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                WriteCatchPostgresExceptions();
                classBuilder.EndNest();
                classBuilder.EndNest();
                WriteReturnEntityIfNotNull(entityVariableName);
                CodeWriterUtils.WriteExitLogging(classBuilder, methodSignature);
            }

            classBuilder.AppendLine("");
        }

        private void WriteCatchPostgresExceptions()
        {
            classBuilder.BeginNest("catch (Exception e)");
            classBuilder.BeginNest($"if (e is Npgsql.PostgresException pge && pge.SqlState == \"23505\")");
            classBuilder.AppendLine("throw new EntityExistsException();");
            classBuilder.EndNest();
            classBuilder.AppendLine("");
            classBuilder.AppendLine("throw;");
            classBuilder.EndNest();
        }

        private void WriteGetListBys()
        {
            var combinations = CodeWriterUtils.GetGetListByColumnCombinations(table)?.ToList();
            combinations?.ForEach(c =>
                {
                    WriteGetListBy(c);
                    classBuilder.AppendLine("");
                });
        }

        private string ConstructSqlQuery(IEnumerable<Parameter> methodParameters, string innerJoinClause, string columnsToReturn)
        {
            var whereClause = GetWhereClauseFromMethodParameters(methodParameters);
            var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
            var sqlCommandText = $"$\"SELECT {columnsToReturn} FROM \\\"{table.Name}\\\" {thisTableAlias}";
            if (!string.IsNullOrEmpty(innerJoinClause))
            {
                sqlCommandText = $"{sqlCommandText} {innerJoinClause}";
            }

            if (!string.IsNullOrEmpty(whereClause))
            {
                sqlCommandText = $"{sqlCommandText} WHERE {whereClause}";
            }

            sqlCommandText = $"{sqlCommandText};\"";
            return sqlCommandText;
        }

        private string GetInnerJoinOrgUnitClause()
        {
            var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
            var orgUnitTableAlias = codeWriterSettings.Namer.NameToAcronym(CodeWriterUtils.CustomerAssetOrganizationTableName);
            return $"INNER JOIN \\\"{CodeWriterUtils.CustomerAssetOrganizationTableName}\\\" {orgUnitTableAlias} ON {thisTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\" = {orgUnitTableAlias}.\\\"{CodeWriterUtils.CustomerAssetOrganizationIDColumnName}\\\"";
        }

        private DatabaseTable GetOrgUnitTable()
        {
            return table.DatabaseSchema.FindTableByName(CodeWriterUtils.CustomerAssetOrganizationTableName);
        }

        private string GetAllColumnNamesByCustomer()
        {
            return GetAllColumnNames(new List<DatabaseTable> { table, GetOrgUnitTable() });
        }

        private void WriteGetLists()
        {
            WriteGetList();
            WriteGetListByCustomer();
        }

        private void WriteGetList()
        {
            var methodParameters = CodeWriterUtils.GetGetListMethodParameters(table, codeWriterSettings, false);
            WriteGetListCommon(methodParameters, null, GetAllColumnNames(new List<DatabaseTable> { table }));
        }

        private void WriteGetListByCustomer()
        {
            var methodParametersByCustomer = CodeWriterUtils.GetGetListMethodParameters(table, codeWriterSettings, true);
            if (methodParametersByCustomer == null || !methodParametersByCustomer.Any())
            {
                return;
            }

            WriteGetListCommon(methodParametersByCustomer, GetInnerJoinOrgUnitClause(), GetAllColumnNamesByCustomer());
        }

        private void WriteBeginReadReaderMultiple()
        {
            classBuilder.BeginNest("while (reader.Read())");
        }

        private void WriteGetListCommon(IEnumerable<Parameter> methodParameters, string innerJoinClause, string columnsToReturn)
        {
            WriteGetListMethodSummary(methodParameters);
            var methodSignature = CodeWriterUtils.GetGetListMethodSignature(table, codeWriterSettings, methodParameters);
            using (classBuilder.BeginNest($"public {methodSignature}"))
            {
                CodeWriterUtils.WriteEntryLogging(classBuilder, methodSignature);
                var sqlCommandText = ConstructSqlQuery(methodParameters, innerJoinClause, columnsToReturn);
                classBuilder.AppendLine($"var entities = new List<{table.NetName}>();");
                WriteBeginCreateConnection();
                WriteBeginCreateCommand();
                classBuilder.AppendLine($"command.CommandText = {sqlCommandText};");
                WriteAddParametersToCommand(methodParameters);
                WriteBeginExecuteReader();
                WriteBeginReadReaderMultiple();
                classBuilder.AppendLine($"var entity = ({table.NetName}){_serviceProviderFieldName}.GetService(typeof({table.NetName}));");
                WriteParseEntityFromReader("entity");
                classBuilder.AppendLine("entities.Add(entity);");
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.AppendLine("");
                classBuilder.AppendLine("return entities;");
                CodeWriterUtils.WriteExitLogging(classBuilder, methodSignature);
            }

            classBuilder.AppendLine("");
        }

        private void WriteGets()
        {
            WriteGet();
            WriteGetByCustomer();
            WriteGetUnique();
            WriteGetUniqueByCustomer();
        }

        private void WriteGet()
        {
            var methodParameters = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, false, false).ToList();
            WriteGetCommon(methodParameters, null, GetAllColumnNames(new List<DatabaseTable> { table }));
        }

        private void WriteGetByCustomer()
        {
            var methodParametersByCustomer = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, true, false);
            if (methodParametersByCustomer == null || !methodParametersByCustomer.Any())
            {
                return;
            }

            WriteGetCommon(methodParametersByCustomer, GetInnerJoinOrgUnitClause(), GetAllColumnNamesByCustomer());
        }

        private void WriteGetUnique()
        {
            var methodParametersUnique = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, false, true);
            if (methodParametersUnique == null || !methodParametersUnique.Any())
            {
                return;
            }

            WriteGetCommon(methodParametersUnique, null, GetAllColumnNames(new List<DatabaseTable> { table }));
        }

        private void WriteGetUniqueByCustomer()
        {
            var methodParametersUniqueByCustomer = CodeWriterUtils.GetGetMethodParameters(table, codeWriterSettings, true, true);
            if (methodParametersUniqueByCustomer == null || !methodParametersUniqueByCustomer.Any())
            {
                return;
            }

            WriteGetCommon(methodParametersUniqueByCustomer, GetInnerJoinOrgUnitClause(), GetAllColumnNamesByCustomer());
        }
        
        private void WriteGetCommon(IEnumerable<Parameter> methodParameters, string innerJoinClause, string columnsToReturn)
        {
            WriteGetMethodSummary(methodParameters);
            var methodSignature = CodeWriterUtils.GetGetMethodSignature(table, codeWriterSettings, methodParameters);
            using (classBuilder.BeginNest($"public {methodSignature}"))
            {
                CodeWriterUtils.WriteEntryLogging(classBuilder, methodSignature);
                var sqlCommandText = ConstructSqlQuery(methodParameters, innerJoinClause, columnsToReturn);
                var entityVariableName = "entity";
                classBuilder.AppendLine($"{table.NetName} {entityVariableName} = null;");
                WriteBeginCreateConnection();
                WriteBeginCreateCommand();
                classBuilder.AppendLine($"command.CommandText = {sqlCommandText};");
                WriteAddParametersToCommand(methodParameters);
                WriteBeginExecuteReader();
                WriteBeginReadReaderSingle();
                // TODO: KE - discuss the following commented block -- I think this is a good idea for us until our schema is made to match business rules
                /*using (cb.BeginNest("if (reader.Read())"))
                {
                    cb.AppendLine("throw new InvalidOperationException(\"Multiple rows match the specified criteria.\");");
                }

                cb.AppendLine("");*/
                classBuilder.AppendLine($"{entityVariableName} = ({table.NetName}){_serviceProviderFieldName}.GetService(typeof({table.NetName}));");
                WriteParseEntityFromReader(entityVariableName);
                // TODO: KE - parse the org unit information coming back and populate the entity's org unit so that WithCustomerAssetOrganization does not have to be called at the service layer, also need to modify returned columns
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                WriteReturnEntityIfNotNull(entityVariableName);
                CodeWriterUtils.WriteExitLogging(classBuilder, methodSignature);
            }

            classBuilder.AppendLine("");
        }

        private void WriteConstructorsAndFields()
        {
            var fields = new List<Parameter>();
            var dbContextParameter = CodeWriterUtils.GetDbContextMethodParameter();
            dbContextParameter.Name = _dbContextFieldName;
            fields.Add(dbContextParameter);
            fields.Add(new Parameter
            {
                DataType = $"ILogger<{CodeWriterUtils.GetRepositoryImplementationName(table)}>",
                Name = _loggerFieldName
            });
            fields.Add(new Parameter
            {
                DataType = "IServiceProvider",
                Name = _serviceProviderFieldName
            });
            WriteFields(fields);
            classBuilder.AppendLine("");
            WriteConstructor(fields);
            classBuilder.AppendLine("");
        }

        private void WriteFields(IEnumerable<Parameter> fields)
        {
            foreach (var f in fields)
            {
                classBuilder.AppendLine($"private {f.DataType} {f.Name};");
            }
        }

        private void WriteConstructor(IEnumerable<Parameter> fields)
        {
            var constructorSignature = string.Join(", ", fields.Select(f => $"{f.DataType} {f.Name}"));
            using (classBuilder.BeginNest($"public {CodeWriterUtils.GetRepositoryImplementationName(table)}({constructorSignature})"))
            {
                foreach (var f in fields)
                {
                    classBuilder.AppendLine($"this.{f.Name} = {f.Name};");
                }
            }
        }

        private void WriteUsings()
        {
            classBuilder.AppendLine("using System;");
            classBuilder.AppendLine("using System.Collections.Generic;");
            classBuilder.AppendLine("using System.ComponentModel.DataAnnotations.Schema;");
            classBuilder.AppendLine("using System.Diagnostics;");
            classBuilder.AppendLine("using System.Linq;");
            classBuilder.AppendLine("using System.Reflection;");
            classBuilder.AppendLine("using Microsoft.Extensions.Logging;");
            if (table.Columns.Select(c => c.DataType.IsGeospatial).Contains(true))
            {
                classBuilder.AppendLine("using NetTopologySuite.Geometries;");
            }

            classBuilder.AppendLine("using PeopleNet.EnterpriseData.DataAccess.Exceptions;");
            foreach (var u in codeWriterSettings.Usings.Where(u => !u.Equals(codeWriterSettings.Namespace)))
            {
                classBuilder.AppendLine($"using {u};");
            }

            classBuilder.AppendLine("");
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

        private void WriteInstantiateEntityFromService()
        {

        }

        private void WriteGetListBy(IEnumerable<DatabaseColumn> columns)
        {
            var methodParameters = CodeWriterUtils.GetMethodParametersForColumns(columns, codeWriterSettings);
            WriteGetListByMethodSummary(methodParameters);
            var methodSignature = CodeWriterUtils.GetGetListByMethodSignature(table, columns, codeWriterSettings, methodParameters);
            using (classBuilder.BeginNest($"public {methodSignature}"))
            {
                CodeWriterUtils.WriteEntryLogging(classBuilder, methodSignature);
                var sqlCommandText = ConstructSqlQuery(methodParameters, null, GetAllColumnNames(new List<DatabaseTable> { table }));
                classBuilder.AppendLine($"var entities = new List<{table.NetName}>();");
                WriteBeginCreateConnection();
                WriteBeginCreateCommand();
                classBuilder.AppendLine($"command.CommandText = {sqlCommandText};");
                WriteAddParametersToCommand(methodParameters);
                WriteBeginExecuteReader();
                WriteBeginReadReaderMultiple();
                classBuilder.AppendLine($"var entity = ({table.NetName}){_serviceProviderFieldName}.GetService(typeof({table.NetName}));");
                WriteParseEntityFromReader("entity");
                classBuilder.AppendLine("entities.Add(entity);");
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.AppendLine("");
                classBuilder.AppendLine("return entities;");
                CodeWriterUtils.WriteExitLogging(classBuilder, methodSignature);
            }
        }

        private string GetWhereClauseFromMethodParameters(IEnumerable<Parameter> methodParameters)
        {
            var whereClauseElements = methodParameters.Select(mp =>
            {
                if (string.IsNullOrEmpty(mp.ColumnNameToQueryBy))
                {
                    throw new InvalidOperationException();
                }

                return $"{mp.TableAlias}.\\\"{mp.ColumnNameToQueryBy}\\\" = @{mp.Name}";
            }).ToList();
            var logicalDeleteColumn = table.Columns.SingleOrDefault(c => logicalDeleteColumns.Contains(c.Name));
            if (logicalDeleteColumn != null)
            {
                var ta = codeWriterSettings.Namer.NameToAcronym(table.Name);
                whereClauseElements.Add($"{ta}.\\\"{logicalDeleteColumn.Name}\\\" IS NULL");
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
            WriteCreateMethodSummary(methodParameters);
            var methodSignature = CodeWriterUtils.GetCreateMethodSignature(table, methodParameters);
            using (classBuilder.BeginNest($"public {methodSignature}"))
            {
                CodeWriterUtils.WriteEntryLogging(classBuilder, methodSignature);
                WriteGetPropertyColumnPairs();
                classBuilder.AppendLine("var valuesClause = string.Join(\", \", propertyColumnPairs.Keys.Select(k => \"@\" + k.Name));");
                var thisTableAlias = codeWriterSettings.Namer.NameToAcronym(table.Name);
                classBuilder.AppendLine($"var sqlCommandText = $\"INSERT INTO \\\"{table.Name}\\\" AS {thisTableAlias} ({{string.Join(\", \", propertyColumnPairs.Values)}}) VALUES ({{valuesClause}}) RETURNING {GetAllColumnNames(new List<DatabaseTable> { table })};\";");
                var entityVariableName = "createdEntity";
                classBuilder.AppendLine($"{table.NetName} {entityVariableName} = null;");
                WriteBeginCreateConnection();
                WriteBeginCreateCommand();
                classBuilder.AppendLine($"command.CommandText = sqlCommandText;");
                WriteAddParametersToCommand();
                classBuilder.BeginNest("try");
                WriteBeginExecuteReader();
                WriteBeginReadReaderSingle();
                classBuilder.AppendLine($"{entityVariableName} = ({table.NetName}){_serviceProviderFieldName}.GetService(typeof({table.NetName}));");
                WriteParseEntityFromReader(entityVariableName);
                classBuilder.EndNest();
                classBuilder.EndNest();
                classBuilder.EndNest();
                WriteCatchPostgresExceptions();
                classBuilder.EndNest();
                classBuilder.EndNest();
                WriteReturnEntityIfNotNull(entityVariableName);
                CodeWriterUtils.WriteExitLogging(classBuilder, methodSignature);
            }

            classBuilder.AppendLine("");
        }

        private string PrintParametersForCall(IEnumerable<Parameter> methodParameters)
        {
            return string.Join(", ", methodParameters.Select(mp => $"{mp.Name}"));
        }


        private string GetAllColumnNames(IEnumerable<DatabaseTable> tables)
        {
            var columnNames = new List<string>();
            foreach (var t in tables)
            {
                var alias = codeWriterSettings.Namer.NameToAcronym(t.Name);
                columnNames.AddRange(t.Columns.Select(c => $"{alias}.\\\"{c.Name}\\\""));
            }

            return string.Join(", ", columnNames);
        }

        private void WriteGetPropertyColumnPairs()
        {
            classBuilder.AppendLine($"var columnProperties = entity.GetType().GetProperties().Where(p => p.IsDefined(typeof(ColumnAttribute), false));");
            classBuilder.AppendLine($"var propertyColumnPairs = new Dictionary<PropertyInfo, string>();");
            using (classBuilder.BeginNest($"foreach (var cp in columnProperties)"))
            {
                classBuilder.AppendLine($"var columnAttribute = (ColumnAttribute)cp.GetCustomAttribute(typeof(ColumnAttribute));");
                classBuilder.AppendLine($"var dbGeneratedAttribute = (DatabaseGeneratedAttribute)cp.GetCustomAttribute(typeof(DatabaseGeneratedAttribute));");
                using (classBuilder.BeginNest($"if (dbGeneratedAttribute != null && dbGeneratedAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)"))
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

        private void WriteBeginExecuteReader()
        {
            classBuilder.BeginNest("using (var reader = command.ExecuteReader())");
        }

        private void WriteBeginCreateCommand()
        {
            classBuilder.BeginNest($"using (var command = connection.CreateCommand())");
        }

        private void WriteBeginCreateConnection()
        {
            classBuilder.BeginNest($"using (var connection = {_dbContextFieldName}.CreateConnection())");
            classBuilder.AppendLine("connection.Open();");
        }

        private void WriteReturnEntityIfNotNull(string entityVariableName)
        {
            classBuilder.AppendLine("");
            using (classBuilder.BeginNest($"if ({entityVariableName} == null)"))
            {
                classBuilder.AppendLine("return null;");
            }

            classBuilder.AppendLine("");
            classBuilder.AppendLine($"return {entityVariableName};");
        }

        private void WriteParseEntityFromReader(string entityVariableName)
        {
            foreach (var c in table.Columns)
            {
                if (c.Nullable)
                {
                    classBuilder.AppendLine($"{entityVariableName}.{CodeWriterUtils.GetPropertyNameForDatabaseColumn(c)} = reader.GetValue({c.Ordinal - 1}) == DBNull.Value ? null : ({CodeWriterUtils.FindDataType(c)})reader.GetValue({c.Ordinal - 1});");
                }
                else
                {
                    classBuilder.AppendLine($"{entityVariableName}.{CodeWriterUtils.GetPropertyNameForDatabaseColumn(c)} = ({CodeWriterUtils.FindDataType(c)})reader.GetValue({c.Ordinal - 1});");
                }
            }
        }
    }
}
