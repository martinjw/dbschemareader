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
        /// <summary>
        /// Uses the specified schema to write class files, NHibernate mapping and a project file. Any existing files are overwritten. If not required, simply discard the mapping and project file. Use these classes as ViewModels in combination with the data access strategy of your choice.
        /// </summary>
        /// <param name="schema">The database schema.</param>
        /// <param name="directory">The directory to write the files to. Will create a subdirectory called "mapping". The directory must exist- any files there will be overwritten.</param>
        /// <param name="namespace">The namespace (optional).</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="IOException"/>
        /// <exception cref="UnauthorizedAccessException"/>
        /// <exception cref="SecurityException" />
        public void Execute(DatabaseSchema schema, DirectoryInfo directory, string @namespace)
        {

            if (schema == null)
                throw new ArgumentNullException("schema");
            if (directory == null)
                throw new ArgumentNullException("directory");
            if (!directory.Exists)
                throw new InvalidOperationException("Directory does not exist");

            //consider exposing this and letting external functions fix the names
            PrepareNames(schema);

            var pw = new ProjectWriter(@namespace);

            var mapping = new DirectoryInfo(Path.Combine(directory.FullName, "mapping"));
            if (!mapping.Exists) mapping.Create();

            foreach (var table in schema.Tables)
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

            File.WriteAllText(
                Path.Combine(directory.FullName, (@namespace ?? "Project") + ".csproj"),
                pw.Write());
        }

        private static void PrepareNames(DatabaseSchema schema)
        {
            foreach (var table in schema.Tables)
            {
                table.NetName = NameFixer.Fix(table.Name);
                foreach (var column in table.Columns)
                {
                    column.NetName = NameFixer.Fix(column.Name);
                    //if it's a foreign key (CategoryId)
                    if (column.IsForeignKey && column.NetName.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        //remove the "Id" - it's just a "Category"
                        var netName = column.NetName;
                        column.NetName = netName.Substring(0, netName.Length - 2);
                    }
                }
            }
        }
    }
}
