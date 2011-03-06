using System;
using System.IO;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen;

namespace DatabaseSchemaViewer
{
    class TaskRunner
    {
        private readonly DatabaseSchema _databaseSchema;

        public TaskRunner(DatabaseSchema databaseSchema)
        {
            _databaseSchema = databaseSchema;
        }

        public bool RunTableDdl(DirectoryInfo directory, SqlType dialect)
        {
            var tg = new DdlGeneratorFactory(dialect).AllTablesGenerator(_databaseSchema);
            tg.IncludeSchema = false;
            string txt;
            try
            {
                txt = tg.Write();
            }
            catch (Exception exception)
            {
                Message =
                    @"An error occurred while creating the script.\n" + exception.Message;
                return false;
            }
            try
            {
                var path = Path.Combine(directory.FullName, "table.sql");
                File.WriteAllText(path, txt);
                Message = @"Wrote " + path;
                return true;
            }
            catch (Exception exception)
            {
                Message =
                    @"An IO error occurred while writing the file.\n" + exception.Message;
            }
                return false;
        }

        public bool RunSprocs(DirectoryInfo directory, SqlType dialect, DatabaseTable table)
        {
            if (table == null)
            {
                Message = "No table";
                return false;
            }

            var gen = new DdlGeneratorFactory(dialect).ProcedureGenerator(table);
            if(gen == null)
            {
                //there is no sproc provider (SQLite)
                Message = @"There is no sproc generator";
                return false;
            }
            var path = Path.Combine(directory.FullName, table.Name + "_sprocs.sql");
            try
            {
                gen.WriteToScript(path);
                Message = @"Wrote " + path;
                return true;
            }
            catch (Exception exception)
            {
                Message =
                    @"An error occurred while creating the script.\n" + exception.Message;
            }
            return false;
        }

        public string Message { get; private set; }

    }
}
