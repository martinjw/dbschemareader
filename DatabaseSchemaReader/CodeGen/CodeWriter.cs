using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DatabaseSchemaReader.CodeGen.CodeFirst;
using DatabaseSchemaReader.CodeGen.NHibernate;
using DatabaseSchemaReader.CodeGen.Procedures;
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
        private readonly ProjectVersion _projectVersion;

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

            var vs2010 = _codeWriterSettings.WriteProjectFile;
            var vs2015 = _codeWriterSettings.WriteProjectFileNet46;
            _projectVersion = vs2015 ? ProjectVersion.Vs2015 : vs2010 ? ProjectVersion.Vs2010 : ProjectVersion.Vs2008;
            //cannot be .net 3.5
            if (IsCodeFirst() && _projectVersion == ProjectVersion.Vs2008) _projectVersion = ProjectVersion.Vs2015;

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
        /// <exception cref="System.Security.SecurityException" />
        public void Execute(DirectoryInfo directory)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (!directory.Exists)
                throw new InvalidOperationException("Directory does not exist: " + directory.FullName);

            var pw = CreateProjectWriter();

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

        /// <summary>
        /// Creates the project writer, using either 2008 or 2010 or VS2015 format.
        /// </summary>
        /// <returns></returns>
        private ProjectWriter CreateProjectWriter()
        {
            var pw = new ProjectWriter(_codeWriterSettings.Namespace, _projectVersion);
            return pw;
        }

        private static string WriteClassFile(DirectoryInfo directory, string className, string txt)
        {
            var fileName = className + ".cs";
            var path = Path.Combine(directory.FullName, fileName);
            if (!directory.Exists) directory.Create();
            File.WriteAllText(path, txt);
            return fileName;
        }

        private void WriteProjectFile(DirectoryInfo directory, ProjectWriter pw)
        {
            if (_codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore)
            {
                //for Core we might be project.json. 
                //Even for csproj Nuget restore is too complex so skip this.
                return;
            }
            var vs2010 = _codeWriterSettings.WriteProjectFile;
            var vs2008 = _codeWriterSettings.WriteProjectFileNet35;
            var vs2015 = _codeWriterSettings.WriteProjectFileNet46;
            if (IsCodeFirst()) vs2008 = false;
            //none selected, do nothing
            if (!vs2010 && !vs2008 && !vs2015) return;

            var projectName = _codeWriterSettings.Namespace ?? "Project";

            File.WriteAllText(
                    Path.Combine(directory.FullName, projectName + ".csproj"),
                    pw.Write());
        }

        private bool IsCodeFirst()
        {
            return _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst ||
                _codeWriterSettings.CodeTarget == CodeTarget.PocoEfCore;
        }

        private bool FilterIneligible(DatabaseTable table)
        {
            if (!IsCodeFirst()) return false;
            if (table.IsManyToManyTable() && _codeWriterSettings.CodeTarget == CodeTarget.PocoEntityCodeFirst)
                return true;
            if (table.PrimaryKey == null)
                return true;
            if (table.Name.Equals("__MigrationHistory", StringComparison.OrdinalIgnoreCase)) //EF 6
                return true;
            if (table.Name.Equals("__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase)) //EF Core1
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

            var packWriter = new PackagesWriter(_projectVersion);
            if (RequiresOracleManagedClient) packWriter.AddOracleManagedClient();

            //no need to reference NHibernate for HBMs
            switch (_codeWriterSettings.CodeTarget)
            {
                case CodeTarget.PocoNHibernateFluent:
                    pw.AddNHibernateReference();
                    var packs = packWriter.WriteFluentNHibernate();
                    WritePackagesConfig(directory, pw, packs);
                    break;
                case CodeTarget.PocoEntityCodeFirst:
                case CodeTarget.PocoRiaServices:
                    pw.AddEntityFrameworkReference();
                    WritePackagesConfig(directory, pw, packWriter.WriteEntityFramework());
                    break;
            }
        }

        private void WritePackagesConfig(FileSystemInfo directory, ProjectWriter pw, string xml)
        {
            pw.AddPackagesConfig();
            File.WriteAllText(
                Path.Combine(directory.FullName, "packages.config"),
                xml);
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
                    //TPT subclasses are mapped in base class
                    if (table.FindInheritanceTable() != null) return;
                    var mw = new MappingWriter(table, _codeWriterSettings);
                    var txt = mw.Write();

                    fileName = table.NetName + ".hbm.xml";
                    var path = Path.Combine(_mappingPath, fileName);
                    File.WriteAllText(path, txt);
                    pw.AddMap(@"mapping\" + fileName);
                    break;
                case CodeTarget.PocoEntityCodeFirst:
                case CodeTarget.PocoRiaServices:
                case CodeTarget.PocoEfCore:
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

        private bool RequiresOracleManagedClient
        {
            get
            {
                var provider = _schema.Provider;
                if (provider == null) return false;
                return provider.StartsWith("Oracle.ManagedDataAccess", StringComparison.OrdinalIgnoreCase);
            }
        }

        private void WriteStoredProcedure(string procedures, string directoryPath, string @namespace, DatabaseStoredProcedure sproc, ProjectWriter pw)
        {
            //if no .net classname, don't process
            if (string.IsNullOrEmpty(sproc.NetName)) return;

            var sw = new SprocWriter(sproc, @namespace);
            var txt = sw.Write();
            var fileName = sproc.NetName + ".cs";
            var path = Path.Combine(directoryPath, fileName);
            File.WriteAllText(path, txt);
            pw.AddClass(procedures + @"\" + fileName);
            if (sw.RequiresOracleReference)
            {
                if (sw.RequiresDevartOracleReference)
                    pw.AddDevartOracleReference();
                else if (RequiresOracleManagedClient)
                    pw.AddOracleManagedReference();
                else
                    pw.AddOracleReference();
            }

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
