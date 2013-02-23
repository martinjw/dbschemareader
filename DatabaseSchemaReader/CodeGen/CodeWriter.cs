using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.CodeGen.NHibernate;
using DatabaseSchemaReader.CodeGen.Procedures;
using DatabaseSchemaReader.Conversion;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// A *simple* code generation
    /// </summary>
    public class CodeWriter
    {
        private readonly DatabaseSchema _schema;
        private string _mappingPath;
        private MappingNamer _mappingNamer;
        private readonly CodeWriterSettings _codeWriterSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeWriter"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public CodeWriter(DatabaseSchema schema)
            : this(schema, new CodeWriterSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeWriter"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="codeWriterSettings">The code writer settings.</param>
        public CodeWriter(DatabaseSchema schema, CodeWriterSettings codeWriterSettings)
        {
            if (schema == null) throw new ArgumentNullException("schema");
            if (codeWriterSettings == null) throw new ArgumentNullException("codeWriterSettings");

            _schema = schema;
            _codeWriterSettings = codeWriterSettings;

            PrepareSchemaNames.Prepare(schema, codeWriterSettings.Namer);
        }

        /// <summary>
        /// Uses the specified schema to write class files, NHibernate/EF CodeFirst mapping and a project file. Any existing files are overwritten. If not required, simply discard the mapping and project file. Use these classes as ViewModels in combination with the data access strategy of your choice.
        /// </summary>
        /// <param name="directory">The directory to write the files to. Will create a subdirectory called "mapping". The directory must exist- any files there will be overwritten.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="SecurityException" />
        public void Execute(DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (!directory.Exists)
                throw new InvalidOperationException("Directory does not exist");

            var pw = new ProjectWriter(_codeWriterSettings.Namespace);

            InitMappingProjects(directory, pw);
            _mappingNamer = new MappingNamer();

            foreach (var table in _schema.Tables)
            {
                if (FilterIneligible(table)) continue;
                var className = table.NetName;
                UpdateEntityNames(className, table.Name);

                var cw = new ClassWriter(table, _codeWriterSettings);
                var txt = cw.Write();

                var fileName = WriteClassFile(directory, className, txt);
                pw.AddClass(fileName);

                WriteMapping(table, pw);
            }

            if (_codeWriterSettings.IncludeViews)
            {
                foreach (var view in _schema.Views)
                {
                    var className = view.NetName;
                    UpdateEntityNames(className, view.Name);

                    var cw = new ClassWriter(view, _codeWriterSettings);
                    var txt = cw.Write();

                    var fileName = WriteClassFile(directory, className, txt);
                    pw.AddClass(fileName);

                    WriteMapping(view, pw);
                }
            }


            string contextName = null;
            if (IsCodeFirst())
            {
                contextName = WriteDbContext(directory, pw);
            }

            //we could write functions (at least scalar functions- not table value functions)
            //you have to check the ReturnType (and remove it from the arguments collections).
            if (_codeWriterSettings.WriteStoredProcedures)
            {
                WriteStoredProcedures(directory.FullName, pw);
                WritePackages(directory.FullName, pw);
            }
            if (_codeWriterSettings.WriteUnitTest)
                WriteUnitTest(directory.FullName, contextName);

            WriteProjectFile(directory, pw);
        }

        private static string WriteClassFile(DirectoryInfo directory, string className, string txt)
        {
            var fileName = className + ".cs";
            var path = Path.Combine(directory.FullName, fileName);
            File.WriteAllText(path, txt);
            return fileName;
        }

        private void WriteProjectFile(DirectoryInfo directory, ProjectWriter pw)
        {
            var vs2010 = _codeWriterSettings.WriteProjectFile;
            var vs2008 = _codeWriterSettings.WriteProjectFileNet35;
            if (IsCodeFirst()) vs2008 = false;
            if (!vs2010 && !vs2008) return;

            var projectName = _codeWriterSettings.Namespace ?? "Project";

            if (vs2008)
            {
                File.WriteAllText(
                    Path.Combine(directory.FullName, projectName + ".csproj"),
                    pw.Write());
            }
            if (vs2010)
            {
                pw.UpgradeTo2010();
                if (vs2008) projectName += ".2010";
                File.WriteAllText(
                    Path.Combine(directory.FullName, projectName + ".csproj"),
                    pw.Write());
            }
        }

        private bool IsCodeFirst()
        {
            return _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst;
        }

        private bool FilterIneligible(DatabaseTable table)
        {
            if (!IsCodeFirst()) return false;
            if (table.IsManyToManyTable())
                return true;
            if (table.PrimaryKey == null)
                return true;
            if (table.Name.Equals("__MigrationHistory", StringComparison.OrdinalIgnoreCase))
                return true;
            if (table.Name.Equals("EdmMetadata", StringComparison.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private void UpdateEntityNames(string className, string tableName)
        {
            if (_mappingNamer.EntityNames.Contains(className))
            {
                Debug.WriteLine("Name conflict! " + tableName + "=" + className);
            }
            else
            {
                _mappingNamer.EntityNames.Add(className);
            }
        }

        private string WriteDbContext(FileSystemInfo directory, ProjectWriter projectWriter)
        {
            var writer = new CodeFirstContextWriter(_codeWriterSettings);
            if (ProviderToSqlType.Convert(_schema.Provider) == SqlType.Oracle)
            {
                writer.IsOracle = true;
                projectWriter.AddDevartOracleReference();
            }
            var databaseTables = _schema.Tables.Where(t => !FilterIneligible(t))
                .ToList();
            if (_codeWriterSettings.IncludeViews)
            {
                databaseTables.AddRange(_schema.Views.OfType<DatabaseTable>());
            }
            var txt = writer.Write(databaseTables);
            var fileName = writer.ContextName + ".cs";
            File.WriteAllText(
                Path.Combine(directory.FullName, fileName),
                txt);
            projectWriter.AddClass(fileName);
            return writer.ContextName;
        }

        private void InitMappingProjects(FileSystemInfo directory, ProjectWriter pw)
        {
            if (_codeWriterSettings.CodeTarget == CodeTarget.Poco) return;

            var mapping = new DirectoryInfo(Path.Combine(directory.FullName, "Mapping"));
            if (!mapping.Exists) mapping.Create();
            _mappingPath = mapping.FullName;

            //no need to reference NHibernate for HBMs
            switch (_codeWriterSettings.CodeTarget)
            {
                case CodeTarget.PocoNHibernateFluent:
                    pw.AddNHibernateReference();
                    break;
                case CodeTarget.PocoEntityCodeFirst:
                case CodeTarget.PocoRiaServices:
                    pw.AddEntityFrameworkReference();
                    pw.UpgradeTo2010(); //you can only use 2010
                    break;
            }
        }

        private void WriteMapping(DatabaseTable table, ProjectWriter pw)
        {
            string fileName;
            switch (_codeWriterSettings.CodeTarget)
            {
                case CodeTarget.PocoNHibernateFluent:
                    fileName = WriteFluentMapping(table);
                    pw.AddClass(@"Mapping\" + fileName);
                    break;
                case CodeTarget.PocoNHibernateHbm:
                    var mw = new MappingWriter(table, _codeWriterSettings);
                    var txt = mw.Write();

                    fileName = table.NetName + ".hbm.xml";
                    var path = Path.Combine(_mappingPath, fileName);
                    File.WriteAllText(path, txt);
                    pw.AddMap(@"mapping\" + fileName);
                    break;
                case CodeTarget.PocoEntityCodeFirst:
                case CodeTarget.PocoRiaServices:
                    var cfmw = new CodeFirstMappingWriter(table, _codeWriterSettings, _mappingNamer);
                    var cfmap = cfmw.Write();

                    fileName = cfmw.MappingClassName + ".cs";

                    var filePath = Path.Combine(_mappingPath, fileName);
                    File.WriteAllText(filePath, cfmap);
                    pw.AddClass(@"Mapping\" + fileName);
                    break;
            }
        }

        private string WriteFluentMapping(DatabaseTable table)
        {
            var fluentMapping = new FluentMappingWriter(table, _codeWriterSettings, _mappingNamer);
            var txt = fluentMapping.Write();
            var fileName = fluentMapping.MappingClassName + ".cs";
            var path = Path.Combine(_mappingPath, fileName);
            File.WriteAllText(path, txt);
            return fileName;
        }

        private void WriteStoredProcedures(string directoryFullName, ProjectWriter pw)
        {
            if (!_schema.StoredProcedures.Any()) return;

            //we'll put stored procedures in a "Procedures" subdirectory
            const string procedures = "Procedures";
            var commands = new DirectoryInfo(Path.Combine(directoryFullName, procedures));
            if (!commands.Exists) commands.Create();
            var ns = _codeWriterSettings.Namespace;
            if (!string.IsNullOrEmpty(ns)) ns += "." + procedures;

            foreach (var sproc in _schema.StoredProcedures)
            {
                WriteStoredProcedure(procedures, commands.FullName, ns, sproc, pw);
            }
        }

        private static void WriteStoredProcedure(string procedures, string directoryPath, string @namespace, DatabaseStoredProcedure sproc, ProjectWriter pw)
        {
            //if no .net classname, don't process
            if (string.IsNullOrEmpty(sproc.NetName)) return;

            var sw = new SprocWriter(sproc, @namespace);
            var txt = sw.Write();
            var fileName = sproc.NetName + ".cs";
            var path = Path.Combine(directoryPath, fileName);
            File.WriteAllText(path, txt);
            pw.AddClass(procedures + @"\" + fileName);
            if (sw.RequiresOracleReference) pw.AddOracleReference();

            if (sw.HasResultClass)
            {
                var rs = new SprocResultWriter(sproc, @namespace);
                txt = rs.Write();
                fileName = rs.ClassName + ".cs";
                path = Path.Combine(directoryPath, fileName);
                File.WriteAllText(path, txt);
                pw.AddClass(procedures + @"\" + fileName);
            }
        }

        private void WritePackages(string directoryFullName, ProjectWriter pw)
        {
            foreach (var package in _schema.Packages)
            {
                if (string.IsNullOrEmpty(package.NetName)) continue;
                if (package.StoredProcedures.Count == 0) continue;

                WritePackage(package, directoryFullName, pw);
            }
        }

        private void WritePackage(DatabasePackage package, string directoryFullName, ProjectWriter pw)
        {
            //we'll put stored procedures in subdirectory
            var packDirectory = new DirectoryInfo(Path.Combine(directoryFullName, package.NetName));
            if (!packDirectory.Exists) packDirectory.Create();
            var ns = _codeWriterSettings.Namespace;
            if (!string.IsNullOrEmpty(ns)) ns += "." + package.NetName;

            foreach (var sproc in package.StoredProcedures)
            {
                WriteStoredProcedure(package.NetName, packDirectory.FullName, ns, sproc, pw);
            }
        }

        private void WriteUnitTest(string directoryFullName, string contextName)
        {
            var tw = new UnitTestWriter(_schema, _codeWriterSettings);
            if (!string.IsNullOrEmpty(contextName)) tw.ContextName = contextName;
            var txt = tw.Write();
            if (string.IsNullOrEmpty(txt)) return;
            var fileName = tw.ClassName + ".cs";
            var path = Path.Combine(directoryFullName, fileName);
            File.WriteAllText(path, txt);
            //not included in project as this is just for demo
        }

    }
}
