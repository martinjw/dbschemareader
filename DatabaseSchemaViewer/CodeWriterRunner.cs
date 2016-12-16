using System;
using System.IO;
using DatabaseSchemaReader.CodeGen;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    class CodeWriterRunner
    {
        private readonly DatabaseSchema _databaseSchema;
        private readonly DirectoryInfo _directory;
        private readonly string _ns;
        private readonly bool _readProcedures;

        public CodeWriterRunner(DatabaseSchema databaseSchema, DirectoryInfo directory, string ns, bool readProcedures)
        {
            _readProcedures = readProcedures;
            _ns = ns;
            _directory = directory;
            _databaseSchema = databaseSchema;
            CodeTarget = CodeTarget.Poco;
        }

        public string Message { get; private set; }
        public bool Result { get; private set; }

        public CodeTarget CodeTarget { get; set; }

        public void RunCodeWriter()
        {
            if (_readProcedures)
            {
                var sprocRunner = new DatabaseSchemaReader.Procedures.ResultSetReader(_databaseSchema);
                sprocRunner.Execute();
            }
            var settings = new CodeWriterSettings { Namespace = _ns, CodeTarget = CodeTarget };
            //these have no UI, but the user can edit the config.
            settings.UseForeignKeyIdProperties = Properties.Settings.Default.CodeGenUseForeignKeyIdProperties;
            if (Properties.Settings.Default.CodeGenUsePluralizingNamer)
            {
                settings.Namer = new PluralizingNamer();
            }
            //if poco, write the sprocs - or if read the sprocs, we can generate
            settings.WriteStoredProcedures = (_readProcedures || CodeTarget == CodeTarget.Poco);
            settings.WriteUnitTest = Properties.Settings.Default.CodeGenWriteUnitTest;
            settings.WriteProjectFileNet46 = Properties.Settings.Default.CodeGenWriteProjectFile;
            settings.IncludeViews = Properties.Settings.Default.CodeGenIncludeViews;
            settings.WriteCodeFirstIndexAttribute = Properties.Settings.Default.CodeGenWriteIndexAttribute;
            var cw = new CodeWriter(_databaseSchema, settings);
            try
            {
                cw.Execute(_directory);
                Message = @"Wrote to " + _directory.FullName;
                Result = true;
                return;
            }
            catch (IOException exception)
            {
                Message =
                    @"An IO error occurred while opening the file.\n" + exception.Message;
            }
            catch (UnauthorizedAccessException exception)
            {
                Message =
                    @"The caller does not have the required permission or path is readonly.\n" + exception.Message;
            }
            Result = false;
        }
    }
}
