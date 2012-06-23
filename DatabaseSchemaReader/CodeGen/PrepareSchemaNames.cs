using System;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    static class PrepareSchemaNames
    {

        public static void Prepare(DatabaseSchema schema)
        {
            //no check for duplicate names
            //if NetNames are already set, they are not overridden

            foreach (var table in schema.Tables)
            {
                if (string.IsNullOrEmpty(table.NetName))
                    table.NetName = NameFixer.ToPascalCase(table.Name);
                foreach (var column in table.Columns)
                {
                    Prepare(column);
                }
            }
            foreach (var view in schema.Views)
            {
                if (string.IsNullOrEmpty(view.NetName))
                    view.NetName = NameFixer.ToPascalCase(view.Name);
                foreach (var column in view.Columns)
                {
                    Prepare(column);
                }
            }
            foreach (var sproc in schema.StoredProcedures)
            {
                PrepareStoredProcedureNames(sproc);
            }
            foreach (var package in schema.Packages)
            {
                if (string.IsNullOrEmpty(package.NetName))
                    package.NetName = NameFixer.ToPascalCase(package.Name);
                foreach (var sproc in package.StoredProcedures)
                {
                    PrepareStoredProcedureNames(sproc);
                }
            }
        }

        private static void Prepare(DatabaseColumn column)
        {
            if (!string.IsNullOrEmpty(column.NetName)) return;

            column.NetName = NameFixer.ToPascalCase(column.Name);
            //if it's a foreign key (CategoryId)
            if (column.IsForeignKey && column.NetName.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
            {
                //remove the "Id" - it's just a "Category"
                var netName = column.NetName;
                column.NetName = netName.Substring(0, netName.Length - 2);
            }
        }

        private static void PrepareStoredProcedureNames(DatabaseStoredProcedure sproc)
        {
            if (string.IsNullOrEmpty(sproc.NetName))
                sproc.NetName = NameFixer.ToPascalCase(sproc.Name);
            foreach (var argument in sproc.Arguments)
            {
                if (string.IsNullOrEmpty(argument.NetName))
                    argument.NetName = NameFixer.ToPascalCase(argument.Name);
            }
        }
    }
}
