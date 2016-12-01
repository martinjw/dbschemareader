using System;
using System.Collections.Generic;
using System.Globalization;
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
                        DatabaseTable fkTable = databaseSchema.FindTableByName(fkTableName);
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
            var dataTypes = new Dictionary<string, DataType>();
            foreach (DataType type in databaseSchema.DataTypes)
            {
                //just in case there are duplicate names
                if (!dataTypes.ContainsKey(type.TypeName)) dataTypes.Add(type.TypeName, type);
            }

            foreach (DatabaseTable table in databaseSchema.Tables)
            {
                UpdateColumnDataTypes(dataTypes, table.Columns);
            }
            foreach (DatabaseView view in databaseSchema.Views)
            {
                UpdateColumnDataTypes(dataTypes, view.Columns);
            }
            foreach (DatabaseStoredProcedure sproc in databaseSchema.StoredProcedures)
            {
                UpdateArgumentDataTypes(dataTypes, sproc);
            }
            foreach (DatabaseFunction function in databaseSchema.Functions)
            {
                UpdateArgumentDataTypes(dataTypes, function);
            }
            foreach (DatabasePackage package in databaseSchema.Packages)
            {
                foreach (DatabaseStoredProcedure sproc in package.StoredProcedures)
                {
                    UpdateArgumentDataTypes(dataTypes, sproc);
                }
                foreach (DatabaseFunction function in package.Functions)
                {
                    UpdateArgumentDataTypes(dataTypes, function);
                }
            }
        }

        private static void UpdateArgumentDataTypes(IDictionary<string, DataType> dataTypes, DatabaseStoredProcedure sproc)
        {
            foreach (DatabaseArgument arg in sproc.Arguments)
            {
                arg.DataType = FindDataType(dataTypes, arg.DatabaseDataType);
            }
        }

        private static void UpdateColumnDataTypes(IDictionary<string, DataType> dataTypes, IEnumerable<DatabaseColumn> columns)
        {
            foreach (DatabaseColumn column in columns)
            {
                if (column.DataType == null)
                {
                    string dbDataType = column.DbDataType;
                    column.DataType = FindDataType(dataTypes, dbDataType);
                }
            }
        }

        private static DataType FindDataType(IDictionary<string, DataType> dataTypes, string dbDataType)
        {
            //quick lookup in dictionary, otherwise has to loop thru

            if (string.IsNullOrEmpty(dbDataType)) return null;
            DataType dt;
            if (dataTypes.TryGetValue(dbDataType, out dt)) return dt;

            var brace = dbDataType.IndexOf('(');
            if (brace > 1)
            {
                dbDataType = dbDataType.Substring(0, brace).ToUpperInvariant();
                if (dataTypes.TryGetValue(dbDataType, out dt)) return dt;
            }

            //TIMESTAMP(9) from Oracle == Timestamp
            dt = dataTypes.Values.FirstOrDefault(dataType => dbDataType.StartsWith(dataType.TypeName, StringComparison.OrdinalIgnoreCase));

            int i;
            if (dt == null && int.TryParse(dbDataType, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                dt = dataTypes.Values.FirstOrDefault(dataType => i.Equals(dataType.ProviderDbType));

            return dt;
        }

    }
}
