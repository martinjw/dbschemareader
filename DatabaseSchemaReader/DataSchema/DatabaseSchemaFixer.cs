using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Fixes up schema internal references
    /// </summary>
    public static class DatabaseSchemaFixer
    {
        /// <summary>
        /// Updates the references of child objects to this database
        /// </summary>
        /// <remarks>
        /// Should be safe to call twice.
        /// </remarks>
        public static void UpdateReferences(DatabaseSchema databaseSchema)
        {
            databaseSchema.Tables.ForEach(delegate (DatabaseTable table)
            {
                table.DatabaseSchema = databaseSchema;
                table.Columns.ForEach(delegate (DatabaseColumn c)
                {
                    //fix the bidirectional references
                    c.DatabaseSchema = databaseSchema;
                    c.Table = table;
                    //foreign keys
                    //if (string.IsNullOrEmpty(c.ForeignKeyTableName)) return;
                    if (c.ForeignKeyTableNames.Count == 0) return;
                    foreach (var fkTableName in c.ForeignKeyTableNames)
                    {
                        DatabaseTable fkTable = databaseSchema.FindTableByName(fkTableName, c.SchemaOwner);
                        if (fkTable == null) continue;
                        c.ForeignKeyTable = fkTable;
                        if (!fkTable.ForeignKeyChildren.Contains(table))
                            fkTable.ForeignKeyChildren.Add(table);
                    }
                });
            });
            //update schema
            databaseSchema.Views.ForEach(delegate (DatabaseView view)
            {
                view.DatabaseSchema = databaseSchema;
                view.Columns.ForEach(delegate (DatabaseColumn c)
                                      {
                                          //fix the bidirectional references
                                          c.DatabaseSchema = databaseSchema;
                                          c.Table = view;
                                      });
            });
            databaseSchema.StoredProcedures.ForEach(delegate (DatabaseStoredProcedure sproc)
            {
                sproc.DatabaseSchema = databaseSchema;
            });
            if (databaseSchema.Packages.Count > 0)
            {
                UpdatePackages(databaseSchema);
            }

            if (databaseSchema.UserDefinedTables.Any())
            {
                databaseSchema.UserDefinedTables.ForEach(udt =>
                {
                    udt.DatabaseSchema = databaseSchema;
                    udt.Columns.ForEach(c =>
                    {
                        c.Table = udt;
                        c.DatabaseSchema = databaseSchema;
                    });
                });
            }
            UpdateDataTypes(databaseSchema);
        }

        private static void UpdatePackages(DatabaseSchema databaseSchema)
        {
            var deletedSprocs = new List<DatabaseStoredProcedure>();
            var deletedFuncs = new List<DatabaseFunction>();
            //find stored procedures that are in packages
            databaseSchema.StoredProcedures.ForEach(delegate (DatabaseStoredProcedure sproc)
            {
                string name = sproc.Package;
                if (name == null) return;
                DatabasePackage package = FindPackage(databaseSchema, name, sproc.SchemaOwner);
                if (!package.StoredProcedures.Contains(sproc))
                {
                    package.StoredProcedures.Add(sproc);
                    deletedSprocs.Add(sproc);
                }
            });
            databaseSchema.Functions.ForEach(delegate (DatabaseFunction function)
            {
                string name = function.Package;
                if (name == null) return;
                DatabasePackage package = FindPackage(databaseSchema, name, function.SchemaOwner);
                if (!package.Functions.Contains(function))
                {
                    package.Functions.Add(function);
                    deletedSprocs.Add(function);
                }
            });
            foreach (var deletedSproc in deletedSprocs)
            {
                //has been moved into a package
                databaseSchema.StoredProcedures.Remove(deletedSproc);
            }
            foreach (var deletedFunc in deletedFuncs)
            {
                //has been moved into a package
                databaseSchema.Functions.Remove(deletedFunc);
            }
        }

        private static DatabasePackage FindPackage(DatabaseSchema databaseSchema, string name, string owner)
        {
            var package = databaseSchema.Packages.Find(
                t2 => t2.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (package == null)
            {
                package = new DatabasePackage();
                package.Name = name;
                package.SchemaOwner = owner;
                databaseSchema.Packages.Add(package);
            }
            return package;
        }

        /// <summary>
        /// Updates the datatypes of child objects to this database
        /// </summary>
        public static void UpdateDataTypes(DatabaseSchema databaseSchema)
        {
            //check if no datatypes loaded
            if (databaseSchema.DataTypes.Count == 0) return;

            //quickly lookup the datatypes
            var finder = new DataTypeFinder(databaseSchema);

            foreach (var udt in databaseSchema.UserDataTypes)
            {
                udt.DataType = finder.Find(udt.DbTypeName);
            }
            foreach (DatabaseTable table in databaseSchema.Tables)
            {
                UpdateColumnDataTypes(finder, table.Columns);
            }
            foreach (DatabaseView view in databaseSchema.Views)
            {
                UpdateColumnDataTypes(finder, view.Columns);
            }
            foreach (DatabaseStoredProcedure sproc in databaseSchema.StoredProcedures)
            {
                UpdateArgumentDataTypes(finder, sproc);
            }
            foreach (DatabaseFunction function in databaseSchema.Functions)
            {
                UpdateArgumentDataTypes(finder, function);
            }
            foreach (DatabasePackage package in databaseSchema.Packages)
            {
                foreach (DatabaseStoredProcedure sproc in package.StoredProcedures)
                {
                    UpdateArgumentDataTypes(finder, sproc);
                }
                foreach (DatabaseFunction function in package.Functions)
                {
                    UpdateArgumentDataTypes(finder, function);
                }
            }

            foreach (var udt in databaseSchema.UserDefinedTables)
            {
                UpdateColumnDataTypes(finder, udt.Columns);
            }
        }

        /// <summary>
        /// Updates the datatypes of specific column list
        /// </summary>
        public static void UpdateDataTypes(IList<DataType> types, IList<DatabaseColumn> columns)
        {
            //check if no datatypes loaded
            if (types.Count == 0) return;

            var finder = new DataTypeFinder(types);

            UpdateColumnDataTypes(finder, columns);
        }

        private static void UpdateArgumentDataTypes(DataTypeFinder finder, DatabaseStoredProcedure sproc)
        {
            foreach (DatabaseArgument arg in sproc.Arguments)
            {
                arg.DataType = finder.Find(arg.DatabaseDataType);
            }
        }

        private static void UpdateColumnDataTypes(DataTypeFinder finder, IEnumerable<DatabaseColumn> columns)
        {
            foreach (DatabaseColumn column in columns)
            {
                if (column.DataType == null)
                {
                    string dbDataType = column.DbDataType;
                    column.DataType = finder.Find(dbDataType);
                }
            }
        }
    }
}