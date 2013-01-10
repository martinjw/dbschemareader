using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    static class PrepareSchemaNames
    {

        public static void Prepare(DatabaseSchema schema, INamer namer)
        {
            //no check for duplicate names
            //if NetNames are already set, they are not overridden

            foreach (var table in schema.Tables)
            {
                if (string.IsNullOrEmpty(table.NetName))
                    table.NetName = namer.Name(table);
                foreach (var column in table.Columns)
                {
                    Prepare(column, namer);
                }
            }
            foreach (var view in schema.Views)
            {
                if (string.IsNullOrEmpty(view.NetName))
                    view.NetName = namer.Name(view);
                foreach (var column in view.Columns)
                {
                    Prepare(column, namer);
                }
            }
            foreach (var sproc in schema.StoredProcedures)
            {
                PrepareStoredProcedureNames(sproc, namer);
            }
            foreach (var package in schema.Packages)
            {
                if (string.IsNullOrEmpty(package.NetName))
                    package.NetName = namer.Name(package);
                foreach (var sproc in package.StoredProcedures)
                {
                    PrepareStoredProcedureNames(sproc, namer);
                }
            }
        }

        private static void Prepare(DatabaseColumn column, INamer namer)
        {
            if (!string.IsNullOrEmpty(column.NetName)) return;

            column.NetName = namer.Name(column);
        }

        private static void PrepareStoredProcedureNames(DatabaseStoredProcedure sproc, INamer namer)
        {
            if (string.IsNullOrEmpty(sproc.NetName))
                sproc.NetName = namer.Name(sproc);
            foreach (var argument in sproc.Arguments)
            {
                if (string.IsNullOrEmpty(argument.NetName))
                    argument.NetName = namer.Name(argument);
            }
        }
    }
}
