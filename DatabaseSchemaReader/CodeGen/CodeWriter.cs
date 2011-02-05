using System;
using System.IO;
using System.Security;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    /// <summary>
    /// A *simple* code generation
    /// </summary>
    public class CodeWriter
    {
        private readonly DatabaseSchema _schema;

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeWriter"/> class.
        /// </summary>
        /// <param name="schema">The schema.</param>
        public CodeWriter(DatabaseSchema schema)
        {
            if (schema == null)
                throw new ArgumentNullException("schema");
            _schema = schema;

            PrepareSchemaNames.Prepare(schema);
        }

        /// <summary>
        /// Uses the specified schema to write class files, NHibernate mapping and a project file. Any existing files are overwritten. If not required, simply discard the mapping and project file. Use these classes as ViewModels in combination with the data access strategy of your choice.
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

            var pw = new ProjectWriter(@namespace);

            var mapping = new DirectoryInfo(Path.Combine(directory.FullName, "mapping"));
            if (!mapping.Exists) mapping.Create();

            foreach (var table in _schema.Tables)
            {
                var className = table.NetName;

                var cw = new ClassWriter(table, @namespace);
                var txt = cw.Write();

                var fileName = className + ".cs";
                var path = Path.Combine(directory.FullName, fileName);
                File.WriteAllText(path, txt);
                pw.AddClass(fileName);

                var mw = new MappingWriter(table, @namespace);
                txt = mw.Write();

                fileName = className + ".hbm.xml";
                path = Path.Combine(mapping.FullName, fileName);
                File.WriteAllText(path, txt);
                pw.AddMap(@"mapping\" + fileName);

            }

            WriteStoredProcedures(directory.FullName, @namespace, pw);
            WritePackages(directory.FullName, @namespace, pw);

            File.WriteAllText(
                Path.Combine(directory.FullName, (@namespace ?? "Project") + ".csproj"),
                pw.Write());
        }

        private void WriteStoredProcedures(string directoryFullName, string @namespace, ProjectWriter pw)
        {
            //we'll put stored procedures in a "Procedures" subdirectory
            const string procedures = "Procedures";
            var commands = new DirectoryInfo(Path.Combine(directoryFullName, procedures));
            if (!commands.Exists) commands.Create();
            if (!string.IsNullOrEmpty(@namespace)) @namespace += "." + procedures;

            foreach (var sproc in _schema.StoredProcedures)
            {
                //if no .net classname, don't process
                if (string.IsNullOrEmpty(sproc.NetName)) continue;

                var sw = new SprocWriter(sproc, @namespace);
                var txt = sw.Write();
                var fileName = sproc.NetName + ".cs";
                var path = Path.Combine(commands.FullName, fileName);
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
                //if no .net classname, don't process
                if (string.IsNullOrEmpty(sproc.NetName)) continue;

                var sw = new SprocWriter(sproc, @namespace);
                var txt = sw.Write();
                var fileName = sproc.NetName + ".cs";
                var path = Path.Combine(packDirectory.FullName, fileName);
                File.WriteAllText(path, txt);
                pw.AddClass(package.NetName + @"\" + fileName);
            }
        }

    }
}
