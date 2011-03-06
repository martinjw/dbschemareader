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
        public DatabaseSchema(string connectionString, string providerName)
        {
            ConnectionString = connectionString;
            Provider = providerName;

            Packages = new List<DatabasePackage>();
            Views = new List<DatabaseView>();
            Users = new List<DatabaseUser>();
            Sequences = new List<DatabaseSequence>();
            Functions = new List<DatabaseFunction>();
            Tables = new List<DatabaseTable>();
            StoredProcedures = new List<DatabaseStoredProcedure>();
            DataTypes = new List<DataType>();
        }

        public List<DataType> DataTypes { get; internal set; }

        public List<DatabaseStoredProcedure> StoredProcedures { get; internal set; }

        public List<DatabasePackage> Packages { get; internal set; }

        public List<DatabaseTable> Tables { get; internal set; }

        public List<DatabaseView> Views { get; internal set; }

        public List<DatabaseUser> Users { get; internal set; }

        public List<DatabaseFunction> Functions { get; internal set; }

        public List<DatabaseSequence> Sequences { get; internal set; }

        public string Provider { get; set; }
        public string ConnectionString { get; set; }
        public string Owner { get; set; }

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
