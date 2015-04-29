using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.CodeGen
{
    static class PrepareSchemaNames
    {

        public static void Prepare(DatabaseSchema schema, INamer namer)
        {
            //now with simple check for duplicate names
            var tableNames = new List<string>();
            //if NetNames are already set, they are not overridden

            foreach (var table in schema.Tables)
            {
                if (string.IsNullOrEmpty(table.NetName))
                    table.NetName = namer.Name(table);
                FixDuplicateName(tableNames, table);
                tableNames.Add(table.NetName);
                foreach (var column in table.Columns)
                {
                    Prepare(column, namer);
                }
            }
            foreach (var view in schema.Views)
            {
                if (string.IsNullOrEmpty(view.NetName))
                    view.NetName = namer.Name(view);
                FixDuplicateName(tableNames, view);
                tableNames.Add(view.NetName);
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

        private static void FixDuplicateName(ICollection<string> tableNames, DatabaseTable table)
        {
            var netName = table.NetName;
            if (!tableNames.Contains(netName)) return;
            //first we try to add the schema as a prefix (eg DboCategory).
            var schemaOwner = NameFixer.ToPascalCase(table.SchemaOwner);
            var name = schemaOwner + netName;
            if (!tableNames.Contains(name))
            {
                table.NetName = name;
                return;
            }
            //let's try suffixes- just count up to 100, and if we find a free one, use it
            for (var i = 0; i < 100; i++)
            {
                name = netName + "1";
                if (!tableNames.Contains(name))
                {
                    table.NetName = name;
                    return;
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
