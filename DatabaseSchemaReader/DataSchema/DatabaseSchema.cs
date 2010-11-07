using System;
using System.Collections.Generic;
using System.Globalization;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents the schema of the database, including all tables, views, stored procedures...
    /// </summary>
    [Serializable]
    public class DatabaseSchema
    {
        public DatabaseSchema()
        {
            Packages = new List<DatabasePackage>();
            Views = new List<DatabaseView>();
            Users = new List<DatabaseUser>();
            Sequences = new List<DatabaseSequence>();
            Functions = new List<DatabaseFunction>();
            Tables = new List<DatabaseTable>();
            StoredProcedures = new List<DatabaseStoredProcedure>();
            DataTypes = new List<DataType>();
        }

        public List<DataType> DataTypes { get; set; }

        public List<DatabaseStoredProcedure> StoredProcedures { get; set; }

        public List<DatabasePackage> Packages { get; set; }

        public List<DatabaseTable> Tables { get; set; }

        public List<DatabaseView> Views { get; set; }

        public List<DatabaseUser> Users { get; set; }

        public List<DatabaseFunction> Functions { get; set; }

        public List<DatabaseSequence> Sequences { get; set; }

        /// <summary>
        /// Updates the references of child objects to this database
        /// </summary>
        public void UpdateReferences()
        {
            Tables.ForEach(delegate(DatabaseTable table)
            {
                table.DatabaseSchema = this;
                table.Columns.ForEach(delegate(DatabaseColumn c)
                {
                    c.DatabaseSchema = this;
                    DatabaseTable fkTable = FindTableByName(c.ForeignKeyTableName);
                    c.ForeignKeyTable = fkTable;
                    if(fkTable != null) fkTable.ForeignKeyChildren.Add(table);
                });
            });
            Views.ForEach(delegate(DatabaseView x)
            {
                x.DatabaseSchema = this;
                x.Columns.ForEach(delegate(DatabaseColumn c) { c.DatabaseSchema = this; });
            });
            StoredProcedures.ForEach(delegate(DatabaseStoredProcedure x)
            {
                x.DatabaseSchema = this;
                x.Arguments.ForEach(delegate(DatabaseArgument c) { c.DatabaseSchema = this; });
            });
            if (Packages.Count > 0)
            {
                StoredProcedures.ForEach(delegate(DatabaseStoredProcedure sproc)
                {
                    string name = sproc.Package;
                    var package = Packages.Find(delegate(DatabasePackage t2) { return t2.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
                    if (package == null)
                    {
                        package = new DatabasePackage();
                        package.Name = name;
                        package.SchemaOwner = sproc.SchemaOwner;
                    }
                    package.StoredProcedures.Add(sproc);
                });
            }
        }

        public DatabaseTable FindTableByName(string name)
        {
            return Tables.Find(delegate(DatabaseTable t2) { return t2.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
        }

        /// <summary>
        /// Updates the datatypes of child objects to this database
        /// </summary>
        public void UpdateDataTypes()
        {
            foreach (DatabaseTable table in Tables)
            {
                UpdateColumnDataTypes(table.Columns);
            }
            foreach (DatabaseView view in Views)
            {
                UpdateColumnDataTypes(view.Columns);
            }
            foreach (DatabaseStoredProcedure sproc in StoredProcedures)
            {
                foreach (DatabaseArgument arg in sproc.Arguments)
                {
                    arg.DataType = FindDataType(arg.DatabaseDataType);
                }
            }
        }

        public void UpdateColumnDataTypes(IEnumerable<DatabaseColumn> columns)
        {
            foreach (DatabaseColumn column in columns)
            {
                column.DataType = FindDataType(column.DbDataType);
            }
        }

        public DataType FindDataType(string dbDataType)
        {
            return DataTypes.Find(delegate(DataType x)
            {
                return x.TypeName.Equals(dbDataType, StringComparison.OrdinalIgnoreCase);
            });
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Tables: {0}, Views: {1}, StoredProcedures: {2}", Tables.Count, Views.Count, StoredProcedures.Count);
        }
    }
}
