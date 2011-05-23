using System;
using System.Collections.Generic;

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
            databaseSchema.Tables.ForEach(delegate(DatabaseTable table)
            {
                table.DatabaseSchema = databaseSchema;
                table.Columns.ForEach(delegate(DatabaseColumn c)
                {
                    //fix the bidirectional references
                    c.DatabaseSchema = databaseSchema;
                    c.Table = table;
                    //foreign keys
                    if (string.IsNullOrEmpty(c.ForeignKeyTableName)) return;
                    DatabaseTable fkTable = databaseSchema.FindTableByName(c.ForeignKeyTableName);
                    c.ForeignKeyTable = fkTable;
                    if (fkTable != null && !fkTable.ForeignKeyChildren.Contains(table))
                        fkTable.ForeignKeyChildren.Add(table);
                });
            });
            //update schema
            databaseSchema.Views.ForEach(delegate(DatabaseView view)
            {
                view.DatabaseSchema = databaseSchema;
                view.Columns.ForEach(delegate(DatabaseColumn c)
                                      {
                                          //fix the bidirectional references
                                          c.DatabaseSchema = databaseSchema;
                                          c.Table = view;
                                      });
            });
            databaseSchema.StoredProcedures.ForEach(delegate(DatabaseStoredProcedure sproc)
            {
                sproc.DatabaseSchema = databaseSchema;
                sproc.Arguments.ForEach(delegate(DatabaseArgument arg) { arg.DatabaseSchema = databaseSchema; });
            });
            if (databaseSchema.Packages.Count > 0)
            {
                UpdatePackages(databaseSchema);
            }
            UpdateDataTypes(databaseSchema);
        }

        private static void UpdatePackages(DatabaseSchema databaseSchema)
        {
            var deletedSprocs = new List<DatabaseStoredProcedure>();
            var deletedFuncs = new List<DatabaseFunction>();
            //find stored procedures that are in packages
            databaseSchema.StoredProcedures.ForEach(delegate(DatabaseStoredProcedure sproc)
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
            databaseSchema.Functions.ForEach(delegate(DatabaseFunction function)
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

            foreach (DatabaseTable table in databaseSchema.Tables)
            {
                UpdateColumnDataTypes(databaseSchema, table.Columns);
            }
            foreach (DatabaseView view in databaseSchema.Views)
            {
                UpdateColumnDataTypes(databaseSchema, view.Columns);
            }
            foreach (DatabaseStoredProcedure sproc in databaseSchema.StoredProcedures)
            {
                UpdateArgumentDataTypes(databaseSchema, sproc);
            }
            foreach (DatabaseFunction function in databaseSchema.Functions)
            {
                UpdateArgumentDataTypes(databaseSchema, function);
            }
            foreach (DatabasePackage package in databaseSchema.Packages)
            {
                foreach (DatabaseStoredProcedure sproc in package.StoredProcedures)
                {
                    UpdateArgumentDataTypes(databaseSchema, sproc);
                }
                foreach (DatabaseFunction function in package.Functions)
                {
                    UpdateArgumentDataTypes(databaseSchema, function);
                }
            }
        }

        private static void UpdateArgumentDataTypes(DatabaseSchema databaseSchema, DatabaseStoredProcedure sproc)
        {
            foreach (DatabaseArgument arg in sproc.Arguments)
            {
                arg.DataType = FindDataType(databaseSchema, arg.DatabaseDataType);
            }
        }

        private static void UpdateColumnDataTypes(DatabaseSchema databaseSchema, IEnumerable<DatabaseColumn> columns)
        {
            foreach (DatabaseColumn column in columns)
            {
                if (column.DataType == null)
                {
                    string dbDataType = column.DbDataType;
                    column.DataType = FindDataType(databaseSchema, dbDataType);
                }
            }
        }

        private static DataType FindDataType(DatabaseSchema databaseSchema, string dbDataType)
        {
            if (string.IsNullOrEmpty(dbDataType)) return null;
            var dt = databaseSchema.DataTypes.Find(dataType => dataType.TypeName.Equals(dbDataType, StringComparison.OrdinalIgnoreCase));
            if (dt == null)
            {
                //TIMESTAMP(9) from Oracle == Timestamp
                dt = databaseSchema.DataTypes.Find(dataType => dbDataType.StartsWith(dataType.TypeName, StringComparison.OrdinalIgnoreCase));
            }
            return dt;
        }
    }
}
