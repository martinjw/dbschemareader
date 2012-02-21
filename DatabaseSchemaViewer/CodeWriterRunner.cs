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
            var cw = new CodeWriter(_databaseSchema, CodeTarget);
            cw.HasReadProcedures = _readProcedures;
            try
            {
                cw.Execute(_directory, _ns);
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
