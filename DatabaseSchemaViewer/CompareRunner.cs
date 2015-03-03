using System;
using DatabaseSchemaReader.Compare;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    class CompareRunner
    {
        public bool RunCompare(DatabaseSchema schema1, DatabaseSchema schema2)
        {
            var comparer = new CompareSchemas(schema1, schema2);
            string txt;
            try
            {
                txt = comparer.Execute();
            }
            catch (Exception exception)
            {
                Message =
                    @"An error occurred while creating the script.\n" + exception.Message;
                return false;
            }

            if (string.IsNullOrEmpty(txt))
            {
                Message = "No differences found";
            }
            else
            {
                using (var scriptForm = new ScriptForm(txt))
                {
                    scriptForm.ShowDialog();
                }
            }
            return true;
        }

        public string Message { get; private set; }

    }
}
