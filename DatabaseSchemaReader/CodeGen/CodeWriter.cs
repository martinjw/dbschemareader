using System;
using System.IO;
using System.Linq;
using System.Security;
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
        private readonly CodeTarget _codeTarget;
        private string _mappingPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeWriter"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public CodeWriter(DatabaseSchema schema)
            : this(schema, CodeTarget.Poco)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeWriter"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        /// <param name="codeTarget">The code target.</param>
        public CodeWriter(DatabaseSchema schema, CodeTarget codeTarget)
        {
            if (schema == null)
                throw new ArgumentNullException("schema");
            _schema = schema;
            _codeTarget = codeTarget;
            PrepareSchemaNames.Prepare(schema);
        }

        /// <summary>
        /// Gets or sets the collection namer.
        /// </summary>
        /// <value>
        /// The collection namer.
        /// </value>
        public ICollectionNamer CollectionNamer { get; set; }

        /// <summary>
        /// Uses the specified schema to write class files, NHibernate/EF CodeFirst mapping and a project file. Any existing files are overwritten. If not required, simply discard the mapping and project file. Use these classes as ViewModels in combination with the data access strategy of your choice.
        /// </summary>
        /// <param name="directory">The directory to write the files to. Will create a subdirectory called "mapping". The directory must exist- any files there will be overwritten.</param>
        /// <param name="namespace">The namespace (optional).</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="SecurityException" />
        public void Execute(DirectoryInfo directory, string @namespace)
        {
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (!directory.Exists)
                throw new InvalidOperationException("Directory does not exist");
            if (CollectionNamer == null)
                CollectionNamer = new CollectionNamer();

            var pw = new ProjectWriter(@namespace);

            InitMappingProjects(directory, pw);

            foreach (var table in _schema.Tables)
            {
                if (_codeTarget == CodeTarget.PocoEntityCodeFirst)
                {
                    if (table.IsManyToManyTable())
                        continue;
                    if (table.PrimaryKey == null)
                        continue;
                }
                var className = table.NetName;

                var cw = new ClassWriter(table, @namespace);
                cw.CodeTarget = _codeTarget;
                cw.CollectionNamer = CollectionNamer;
                var txt = cw.Write();

                var fileName = className + ".cs";
                var path = Path.Combine(directory.FullName, fileName);
                File.WriteAllText(path, txt);
                pw.AddClass(fileName);

                WriteMapping(table, @namespace, pw);
            }
            string contextName = null;
            if (_codeTarget == CodeTarget.PocoEntityCodeFirst)
            {
                contextName = WriteDbContext(directory, @namespace, pw);
            }

            //we could write functions (at least scalar functions- not table value functions)
            //you have to check the ReturnType (and remove it from the arguments collections).
            WriteStoredProcedures(directory.FullName, @namespace, pw);
            WritePackages(directory.FullName, @namespace, pw);
            WriteUnitTest(directory.FullName, @namespace, contextName);

            File.WriteAllText(
                Path.Combine(directory.FullName, (@namespace ?? "Project") + ".csproj"),
                pw.Write());
            pw.UpgradeTo2010();
            File.WriteAllText(
                Path.Combine(directory.FullName, (@namespace ?? "Project") + ".2010.csproj"),
                pw.Write());
        }

        private string WriteDbContext(FileSystemInfo directory, string ns, ProjectWriter projectWriter)
        {
            var writer = new CodeFirstContextWriter(ns);
            var txt = writer.Write(_schema.Tables);
            var fileName = writer.ContextName + ".cs";
            File.WriteAllText(
                Path.Combine(directory.FullName, fileName),
                txt);
            projectWriter.AddClass(fileName);
            return writer.ContextName;
        }

        private void InitMappingProjects(FileSystemInfo directory, ProjectWriter pw)
        {
            if (_codeTarget == CodeTarget.Poco) return;

            var mapping = new DirectoryInfo(Path.Combine(directory.FullName, "Mapping"));
            if (!mapping.Exists) mapping.Create();
            _mappingPath = mapping.FullName;

            //no need to reference NHibernate for HBMs
            switch (_codeTarget)
            {
                case CodeTarget.PocoNHibernateFluent:
                    pw.AddNHibernateReference();
                    break;
                case CodeTarget.PocoEntityCodeFirst:
                    pw.AddEntityFrameworkReference();
                    pw.UpgradeTo2010(); //you can only use 2010
                    break;
            }
        }

        private void WriteMapping(DatabaseTable table, string @namespace, ProjectWriter pw)
        {
            string fileName;
            switch (_codeTarget)
            {
                case CodeTarget.PocoNHibernateFluent:
                    fileName = WriteFluentMapping(table, @namespace);
                    pw.AddClass(@"Mapping\" + fileName);
                    break;
                case CodeTarget.PocoNHibernateHbm:
                    var mw = new MappingWriter(table, @namespace);
                    mw.CollectionNamer = CollectionNamer;
                    var txt = mw.Write();

                    fileName = table.NetName + ".hbm.xml";
                    var path = Path.Combine(_mappingPath, fileName);
                    File.WriteAllText(path, txt);
                    pw.AddMap(@"mapping\" + fileName);
                    break;
                case CodeTarget.PocoEntityCodeFirst:
                    var cfmw = new CodeFirstMappingWriter(table, @namespace);
                    cfmw.CollectionNamer = CollectionNamer;
                    var cfmap = cfmw.Write();

                    fileName = table.NetName + "Mapping.cs";
                    var filePath = Path.Combine(_mappingPath, fileName);
                    File.WriteAllText(filePath, cfmap);
                    pw.AddClass(@"Mapping\" + fileName);
                    break;
            }
        }

        private string WriteFluentMapping(DatabaseTable table, string @namespace)
        {
            var fluentMapping = new FluentMappingWriter(table, @namespace);
            fluentMapping.CollectionNamer = CollectionNamer;
            var txt = fluentMapping.Write();
            var fileName = table.NetName + "Mapping.cs";
            var path = Path.Combine(_mappingPath, fileName);
            File.WriteAllText(path, txt);
            return fileName;
        }

        private void WriteStoredProcedures(string directoryFullName, string @namespace, ProjectWriter pw)
        {
            if (!_schema.StoredProcedures.Any()) return;

            //we'll put stored procedures in a "Procedures" subdirectory
            const string procedures = "Procedures";
            var commands = new DirectoryInfo(Path.Combine(directoryFullName, procedures));
            if (!commands.Exists) commands.Create();
            if (!string.IsNullOrEmpty(@namespace)) @namespace += "." + procedures;

            foreach (var sproc in _schema.StoredProcedures)
            {
                WriteStoredProcedure(procedures, commands.FullName, @namespace, sproc, pw);
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

        private void WritePackages(string directoryFullName, string @namespace, ProjectWriter pw)
        {
            foreach (var package in _schema.Packages)
            {
                if (string.IsNullOrEmpty(package.NetName)) continue;
                if (package.StoredProcedures.Count == 0) continue;

                WritePackage(package, directoryFullName, @namespace, pw);
            }
        }

        private static void WritePackage(DatabasePackage package, string directoryFullName, string @namespace, ProjectWriter pw)
        {
            //we'll put stored procedures in subdirectory
            var packDirectory = new DirectoryInfo(Path.Combine(directoryFullName, package.NetName));
            if (!packDirectory.Exists) packDirectory.Create();
            if (!string.IsNullOrEmpty(@namespace)) @namespace += "." + package.NetName;

            foreach (var sproc in package.StoredProcedures)
            {
                WriteStoredProcedure(package.NetName, packDirectory.FullName, @namespace, sproc, pw);
            }
        }

        private void WriteUnitTest(string directoryFullName, string @namespace, string contextName)
        {
            var tw = new UnitTestWriter(_schema, @namespace, _codeTarget);
            if (!string.IsNullOrEmpty(contextName)) tw.ContextName = contextName;
            tw.CollectionNamer = CollectionNamer;
            var txt = tw.Write();
            if (string.IsNullOrEmpty(txt)) return;
            var fileName = tw.ClassName + ".cs";
            var path = Path.Combine(directoryFullName, fileName);
            File.WriteAllText(path, txt);
            //not included in project as this is just for demo
        }

    }
}
