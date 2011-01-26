using System;
using System.Collections.Generic;
using System.Globalization;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// The parent of all schema objects.
    /// </summary>
    /// <remarks>
    /// When initially populated, many of the objects (tables, stored procedures) are not linked.
    /// Use <see cref="DatabaseSchemaFixer.UpdateReferences"/> to link things up
    /// </remarks>
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

 

        public DatabaseTable FindTableByName(string name)
        {
            return Tables.Find(delegate(DatabaseTable t2) { return t2.Name.Equals(name, StringComparison.OrdinalIgnoreCase); });
        }


        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Tables: {0}, Views: {1}, StoredProcedures: {2}", Tables.Count, Views.Count, StoredProcedures.Count);
        }
    }
}
