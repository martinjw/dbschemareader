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
                //find stored procedures that are in packages
                databaseSchema.StoredProcedures.ForEach(delegate(DatabaseStoredProcedure sproc)
                {
                    string name = sproc.Package;
                    if (name == null) return;
                    var package = databaseSchema.Packages.Find(
                        t2 => t2.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                    if (package == null)
                    {
                        package = new DatabasePackage();
                        package.Name = name;
                        package.SchemaOwner = sproc.SchemaOwner;
                    }
                    if (!package.StoredProcedures.Contains(sproc))
                        package.StoredProcedures.Add(sproc);
                });
            }
        }

        /// <summary>
        /// Updates the datatypes of child objects to this database
        /// </summary>
        public static void UpdateDataTypes(DatabaseSchema databaseSchema)
        {
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
                foreach (DatabaseArgument arg in sproc.Arguments)
                {
                    arg.DataType = FindDataType(databaseSchema, arg.DatabaseDataType);
                }
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
            return databaseSchema.DataTypes.Find(dataType => dataType.TypeName.Equals(dbDataType, StringComparison.OrdinalIgnoreCase));
        }
    }
}
