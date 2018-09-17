using System;
using System.IO;
using System.Linq;

namespace DatabaseSchemaReader.CodeGen
{
    public class CodeWriterSettings
    {
        private INamer _namer;
        private string[] _usings;

        public CodeWriterSettings()
        {
            Namespace = "Domain";
            CodeTarget = CodeTarget.Poco;
            var namer = new Namer();
            Namer = namer;
            CodeInserter = new CodeInserter();
        }

        public CodeWriterSettings(DirectoryInfo outputDirectory) : this()
        {
            OutputDirectory = outputDirectory;
        }

        public DirectoryInfo OutputDirectory { get; set; }
        public string Namespace { get; set; }

        public string[] Usings
        {
            get => _usings.Where(u => !u.Equals(Namespace)).ToArray();
            set => _usings = value;
        }

        public CodeTarget CodeTarget { get; set; }
        public INamer Namer
        {
            get { return _namer; }
            set
            {
                if (value == null) throw new ArgumentNullException();
                _namer = value;
            }
        }

        public CodeInserter CodeInserter { get; set; }
        public string RequiredErrorMessage { get; set; }
        public string StringLengthErrorMessage { get; set; }
        public string RangeErrorMessage { get; set; }
        public bool UseForeignKeyIdProperties { get; set; }
        public bool WriteStoredProcedures { get; set; }
        public bool WriteUnitTest { get; set; }
        public bool WriteProjectFile { get; set; }
        public bool WriteProjectFileNet35 { get; set; }
        public bool WriteProjectFileNet46 { get; set; }
        public bool IncludeViews { get; set; }
        public bool WriteCodeFirstIndexAttribute { get; set; }
    }
}
